# IAV22-de-Kadt
Proyecto final individual - Emile de Kadt
## Índice

1. [Introducción](#1-introducci%C3%B3n)

2. [Punto de partida](#2-punto-de-partida)

3. [Tareas](#3-tareas)

4. [Restricciones](#4-restricciones)

5. [Planteamiento](#5-planteaminto)

6. [Proceso](#6-proceso)

7. [Utilización](#7-utilización)

8. [Vídeo](#8-vídeo)

9. [Errores](#9-errores)

10. [Bibliografía](#10-bibliografía)

## 1. Introducción
El objetivo de este proyecto es construir una clase "Agente" alternativa a la presentada en la <a href="https://github.com/IAV22-G09/P1">práctica 1 (El Flautista de Hamelin)</a>. Esta versión del agente está pensada para actores con varias acciones breves, varias de ellas con tiempos de recarga, todas con acceso al mismo juego de percepción sensorial. 

## 2. Punto de partida
Este proyecto se basa en una versión primitiva del mismo concepto, en c++, que creé para el motor de juegos <a href="https://github.com/Triturados/Motor">LOVE</a>. Tras haber visto algunas de las limitaciones de esa primera versión (dificultad de uso, elementos poco intuitivos) pondré un mayor enfoque en la usabilidad de esta nueva versión.

El funcionamiento de esta herramienta se basa en una clase Agent (que era un componente del motor) y su clase interna Action. Agent hace de máquina de estados del actor, abstrayendo la creación y destrucción de las acciones y reproduciendo siempre la que corresponde. Las acciones tienen varias formas de uso, como microcomponentes en su propio derecho. Principalmente se utilizan sobrecargando sus métodos virtuales OnActionStart, ActiveUpdate, PassiveUpdate y ConditionsFulfilled. OnActionStart contiene código que se ejecuta cada vez que se inicia la acción; ActiveUpdate contiene el código que se ejecutará cada frame mientras la acción se esté realizando; y PassiveUpdate contiene código que se ejecutará cada frame, se esté realizando o no la acción (esto se usa principalmente para controlar la prioridad de la acción). Por último, ConditionsFulfilled contiene las condiciones booleanas que necesitan cumplirse para que la acción se comience.

### Código original en c++
```c++
class Agent : public Component
		{
        protected:
            class Action
            {
            public:
                friend Agent;
                // Cantidad de prioridad que se incrementa cada frame
                float increasePrioOverTime = 0.0;

                // Avisa al agente de que NO SE PUEDE tomar otra acción
                bool lockAction = false;

                // Condiciones que se tienen que cumplir para que la acción se realice, ignorando prioridad
                // Por ejemplo un ataque melé puede requerir una mínima proximidad al objetivo
                virtual bool conditionsFulfilled() const { return true; };

                virtual void onActionStart() {};

                Action(Agent* agent_, float priority_ = LONG_MAX);

                // llamado en cada frame, esté o no activa la acción (necesario para control de prioridad etc.)
                virtual void passiveUpdate() {};

                // llamado en cada frame mientras se esté ejecutando la acción
                virtual void activeUpdate() {};

                // Si la acción no se ha completado, tiene que seguir a pesar de las prioridades actuales
                // Si las condiciones no se cumplen, no se ejecutará esta acción
                float getPriority() const noexcept;
            protected:
                Agent* agent;

                // Para incrementar la prioridad, hay que pasar valores negativos
                virtual void addPriority(float increase);
                virtual void setPriority(float priority_);
            private:
                float priority;
                // Este método es privado para evitar que sobrecargar el passiveUpdate requiera actualizar la prioridad
                void passiveUpdateAndPrio();
            };

            // la acción que se está realizando actualmente
            Action* currentAction = nullptr;
        public:
            Agent();

            ~Agent();

            void init() override;

            void update() override;

            Action* addAction(Action* a);

        private:
            // vector iterable de acciones, para llamar los updates
            std::vector<Action*> actions;

            Action* getNextAction();
		};
```

#### Agent.cpp
```cpp
Agent::Agent()
        {
            actions = std::vector<Action*>();
        }

        Agent::~Agent()
        {
            for (Action* a : actions)
                delete a;
        }

        void Agent::init()
        {
            getNextAction();
            if (currentAction == nullptr)
                throw new std::exception("Agente sin acciones");
        }

        void Agent::update()
        {
            currentAction->activeUpdate();

            for (Action* a : actions)
                a->passiveUpdateAndPrio();

            if (!currentAction->lockAction)
            {
                getNextAction();
                currentAction->onActionStart();
            }
        }

        Agent::Action* Agent::addAction(Action* a)
        {
            actions.push_back(a);
            return a;
        }

        Agent::Action* Agent::getNextAction()
        {
            float highestPrio = LONG_MAX;
            
            for (Action* a : actions)
            {
                if (a->getPriority() < highestPrio)
                {
                    currentAction = a;
                    highestPrio = a->getPriority();
                }
            }
            return currentAction;
        }
        Agent::Action::Action(Agent* agent_, float priority_)
        {
            agent = agent_;
            priority = priority_;
        }

        // Si la acción no se ha completado, tiene que seguir a pesar de las prioridades actuales
        // Si las condiciones no se cumplen, no se ejecutará esta acción
        float Agent::Action::getPriority() const noexcept { return lockAction ? LONG_MIN : conditionsFulfilled() ? priority : LONG_MAX; }

        // Para incrementar la prioridad, hay que pasar valores negativos
        void Agent::Action::addPriority(float increase) { priority += increase; }

        void Agent::Action::setPriority(float priority_) {
            priority = priority_;
        }
        void Agent::Action::passiveUpdateAndPrio()
        {
            passiveUpdate();
            addPriority(-increasePrioOverTime * LoveEngine::Time::getInstance()->deltaTime);
        }
```

## 3. Tareas
1. Adaptar el código original a Unity y C#.
2. Mejorar la usabilidad y el encapsulamiento de la clase.
3. Crear interfaz de usuario simple y comprensible.
4. Documentar modo de uso.
4. Integrar soporte para algunas funcionalidades de Unity independientes.
5. Crear ejemplos de funcionamiento.

## 4. Restricciones
A la hora de desarrollar este proyecto es obligatorio:
· Utilizar únicamente las herramientas de Unity y opcionalmente los plugins de terceros
acordados con el profesor, sin reutilizar código ajeno a estos.
· Documentar claramente los algoritmos, heurísticas o cualquier “truco” utilizado.
· Diseñar y programar de la manera más limpia y elegante posible, separando la parte
visual e interactiva del juego, del modelo y las técnicas de IA implementadas.
· Evitar, en la medida de lo posible, el uso de recursos audiovisuales pesados o ajenos.

## 5. Planteamiento

Para mejorar la usabilidad del código, se ocultará todo el sistema de prioridades de las acciones. En su lugar, El usuario podrá establecer una acción *Default* y varias acciones adicionales, que puede tener periodos de enfriamiento (llamados de aquí en adelante *cooldowns*), concatenación (acciones que tienen que ir en un orden específico) y condiciones de bloqueo.

El agente se expandirá con más miembros opcionales que puedan ser utilizados por todas sus acciones en común, tales como un vector de gameobjects objetivos.

Se añadirá un sistema de cooldown más simple de entender y utilizar.

Se añadirá toda clase de comprobaciones de funcionamiento y utilización correctas, lanzando errores descriptivos siempre que sea necesario.

## 6. Proceso

NOTA: Dado que este proyecto se trata en parte de una modificación de código existente en un lenguaje a código en otro lenguaje, veo poco prudente incluir recortes de pseudocódigo, ya que las diferencias de los lenguajes son importantes en este contexto.

### Escenario
Para poder hacer pruebas rápidamente, creé una escena con un recinto cerrado y dos cápsulas de colores: una azul, que usaba el <a href="https://iqcode.com/code/csharp/unity-player-movement-script-3d">controlador de personaje de Max Schulz</a>, y una roja, que llevaría las versiones del agente a probar.

### Cooldowns
El sistema de prioridades variables con el tiempo que usaba el código original era muy poco intuitivo para el uso. Eliminé todo el código relacionado al incremento de prioridad temporal, y lo cambié por un sistema de cooldown. Desde el punto de vista del usuario, solo hay que escribir la línea 
```c#
cooldown = 3f;
```
en el momento deseado para poner la acción en cooldown durante tres segundos.

### Acción base
Para añadir robustez a la clase, implementé las acciones base (DefaulAction). Se trata de asegurar que haya al menos una acción del agente que esté disponible en todo momento. Internamente, esta puede ser cualquier acción que tenga prioridad 0, ninguna precondición y ningún cooldown. Adicionalmente, una vez una acción esté marcada como acción base, no se le puede asignar ningún cooldown. El agente se asegura de siempre tener una sola acción base.
Hay un método público de las acciones que les asigna todos los valores necesarios para ser compatibles como acción base, pero para asignarla como acción base de un agente, hay que llamar al agente.
Hay varios chequeos para asegurar que se haga un uso correcto de estas acciones, lanzándose una excepción particular por cada incumplimiento de este.

### Diccionario de objetos compartidos
Un problema que me estaba encontrando en mis pruebas era que muchas acciones de un mismo agente necesitaban referencias a la misma serie de objetos. Para evitar que el usuario tuviese que manualmente escribir referencias en cada acción, y pedírselas al agente con un cast, añadí a la clase agente base una lista de objetos a los que todas sus acciones podrían acceder.
Esto funcionaba bien, pero requería una revisión frecuente del índice de cada objeto en la lista. Para hacerlo más cómodo, cambié la lista por un diccionario.
La clase Dictionary de c# no es serializable en Unity. Para poder rellenar el diccionario desde el editor, tuve que implementar la clase SerializableDictionary, que hace de envoltorio a un diccionario de strings a GameObjects.

El uso de este diccionario es el siguiente:
Desde el editor, asignar tantos gameobjects y strings como se desee en la propiedad SharedObjects del agente.
Desde el código de cualquier acción, hacer la llamada GetSharedObject("Nombre del objeto")

Si la lista incluye nombres u objetos vacíos, o si las dos listas tienen diferente longitud, se lanza una excepción descriptiva.

### Acciones encadenadas
Ciertos actores tienen acciones que solo tienen sentido estratégico ser usadas detrás de otras. Ya sería posible programar esto usando las condiciones de cada acción, pero para agilizar mucho el proceso he implementado la funcionalidad de acciones encadenadas (set-up y follow-up).
Para hacer una cadena con las acciones A y B, es tan sencillo como escribir la siguiente línea desde el AgentStart() del agente:
```c#
B.AddSetupAction(A);
```
Esto hace que la acción B no se realice hasta que se realice primero la A. Las acciones encadenadas necesitan que se ejecuten todos sus precursores cada vez que se realicen ellas mismas.
Además, se puede añadir un segundo argumento, que indica el número de veces que se tiene que realizar A antes de que se pueda realizar B:
```c#
B.AddSetupAction(A, 3);
```

La estructura interna de las acciones encadenadas es la siguiente. Para ahorrar eficiencia, no se consulta el estado de todas las acciones precursoras en cada frame; cuando una acción empieza, notifica a todas sus acciones dependientes (de ahí la necesidad de tener dos listas) y solo entonces se comprueba el estado de cada acción precursora. Si se detecta que todas las precursoras se han realizado, se actualiza el valor de allSetUpsComplete, que sí se consulta en cada frame.

```c#
/// <summary>
/// Adds an action that must be performed at least once before each time this action can be performed.
/// To require multiple uses of the action beforehand, use the second parameter.
/// </summary>
public void AddSetupAction(Action action, uint n = 1)
{
    allSetUpsComplete = false;

    SetUpAction setUp;
    setUp.action = action;
    setUp.necessaryCount = n;
    setUp.count = 0;
    setUpActions.Add(setUp);

    action.AddFollowUpAction(this);
}

/// <summary>
/// Adds an action that must notify this one each time it is performed
/// </summary>
/// <param name="action"></param>
protected void AddFollowUpAction(Action action)
{
    followUpActions.Add(action);
}


/// <summary>
/// Notifies all follow-up action of this action starting
/// </summary>
public void NotifyChains()
{
    foreach(Action a in followUpActions)
    {
        a.OnSetupAction(this);
    }
}

/// <summary>
/// Upon starting the action, reset the count of all set up actions
/// </summary>
public void ResetSetUp()
{
    allSetUpsComplete = setUpActions.Count == 0;
    for (int i = 0; i < setUpActions.Count; ++i)
        setUpActions[i].Reset();
}

/// <summary>
/// When one of the action's set-ups begins, check to see if action is available now
/// </summary>
/// <param name="a"></param>
protected void OnSetupAction(Action a)
{
    int i = 0;
    while (i < setUpActions.Count && setUpActions[i].action != a) { ++i; }
    if (i == setUpActions.Count) throw new System.Exception("Action notified by unrecognized set-up action.");

    if ((++setUpActions[i]).count >= setUpActions[i].necessaryCount)
    {
        allSetUpsComplete = true;
        foreach (SetUpAction setUp in setUpActions)
            if (setUp.count < setUp.necessaryCount)
            {
                allSetUpsComplete = false;
                break;
            }
    }
}

/// <summary>
/// List of actions that can´t be performed before this one
/// </summary>
List<Action> followUpActions = new List<Action>();

struct SetUpAction
{
    public Action action;
    public uint necessaryCount;
    public uint count;
    public void Reset()
    {
        count = 0;
    }
    private SetUpAction(Action a, uint _necessary, uint _count) { action = a; necessaryCount = _necessary; count = _count; }
    public static SetUpAction operator ++(SetUpAction a) => new SetUpAction(a.action, a.necessaryCount, a.count + 1);
}
/// <summary>
/// List of actions that must be performed at least once before each time this one is
/// </summary>
List<SetUpAction> setUpActions = new List<SetUpAction>();

/// <summary>
/// Indicated whether all set-up action have been performed enough times
/// </summary>
private bool allSetUpsComplete = true;
```

## 7. Utilización
### Inicio
Para utilizar la herramienta en un proyecto de Unity, se deben incluir los dos archivos de código Agent.cs y SerializableDictionary.cs.

### Definición de agentes y acciones
Para definir un agente nuevo, se crea una clase que herede de Agent, dentro del namespace AggressiveAgent. Dentro de esta clase, y como clases protegidas, se definen todas las acciones, heredando cada una de Action.
Cada acción solo tiene que sobrecargar los métodos deseados para su funcionamiento, de entre los siguientes:
```c#
bool Conditions();  // Condiciones que se tienen que satisfacer para que la acción sea elegible
void OnActionStart();   // Llamado cada vez que la acción se realiza
void PassiveUpdate();   // Llamado cada frame. Útil para acciones con prioridades variables
void ActiveUpdate();    // Llamado cada frame mientras la acción está en curso
void OnCollision(Collision collision); // Llamado cuando el objeto agente detecta una colisión mientras la acción esté en curso
void OnTrigger(Collider other); // Llamado cuando el objeto agente detecta una colisión de tipo trigger mientras la acción esté en curso
```

Una vez estén definidas todas las acciones, en el AgentStart() o el AgentAwake() del agente se deben añadir todas a la lista de acciones, con la función AddAction(Action a). Una de las acciones debe estar definida como acción base, usando AddDefaultAction(Action a) en lugar de AddAction.
Las acciones por defecto no pueden cambiar de prioridad ni tener cooldowns.

La estructura de la clase queda de la siguiente forma:
```c#


namespace AggressiveAgent
{
    public class MyAgent : Agent
    {
        protected class MyActionA : Action
        {
            MyActionA(Agent agent_) : Action(agent_, 0)
            ...
        }
        
        protected class MyActionB : Action
        {
            MyActionB(Agent agent_) : Action(agent_, -3)
            ...
        }
        
        protected class MyActionC : Action
        {
            MyActionC(Agent agent_) : Action(agent_, -5)
            ...
        }
        
        override protected void AgentStart()
        {
            AddDefaultAction(new MyActionA(this));
            actionB = (MyActionB)(AddAction(new MyActionB(this)));
            AddAction(new MyActionC(this)).AddSetupAction(actionB, 3);
        }
    }
}
```
En el ejemplo anterior, la acción A solo se realizará si las acciones B y C están simultáneamente en cooldown, o no se cumplen sus condiciones. La acción C, a pesar de ser más prioritaria, no se realizará hasta que la acción B se haya realizado 3 veces (explicado en el apartado Acciones Encadenadas).

### Prioridades
En la constructora de la acción se debe definir su prioridad (0 por defecto) aunque esta se puede cambiar más adelante. 0 es la prioridad de la acción base, y un valor menor se considera más prioritario. Por ejemplo, si hay tres acciones definidas: A, con prioridad 0 (acción base); B, con prioridad -5; y C, con prioridad -20, se relizará primero la C.

### Bloqueo de Acciones
Por defecto, cualquier acción puede interrumpir otra. Esto es conveniente en algunos casos (interrumpir un movimiento para atacar, por ejemplo). Pero en otros casos, para evitar esto, se puede bloquear el cambio de acción con el miembro lockAction y el método UnlockIn.
```c#
lockAction = true;  // bloquea el cambio de acción
lockAction = false; // habilita de nuevo el cambio de acción
UnlockIn(3f); // desbloquea la acción al cabo de 3 segundos.
```

### Cooldowns
Para que una acción no se pueda realizar constantemente, se le puede asignar un tiempo de enfriamiento. Con la línea 
```c#
cooldown = 3f;
```
se deshabilita la acción por 3 segundos.

### Acciones Encadenadas
Aparte del método de condiciones, que es abierto, se proporciona la facilidad de tener acciones que tienen que suceder a otras. Esto se hace con el método AddSetupAction(Action a, int count). Este método hace que una acción no pueda realizarse hasta que la acción que se pase como argumento se haya realizado tantas veces como indique el segundo argumento. Una acción puede tener varias acciones previas, cada una definida aparte.
Cada vez que se realice una acción, se reinicia la cuenta de todas sus acciones previas.

### Objetos Compartidos
Frecuentemente, varias de las acciones de un mismo agente requieren referencias a los mismos objetos. Para no tener que declarar campos públicos en el agente y pasarlos a cada acción, existe un campo en todos los agentes llamado Shared Objects. 
Este campo es visible en el editor, y consta de dos sub-campos: una lista de strings y otra de GameObjects. Desde el editor se pueden llenar estas dos listas con objetos de la escena y claves para estos.
Para usar estos objetos desde el código de una acción, se llama al método:
```c#
GameObject o = GetSharedObject("ObjectName");
```

### Funcionalidades Adicionales
Para pillar una referencia al Transform del objeto agente, simplemente escribe transform.

Todas las acciones tienen una referencia al Rigidbody del objeto, si tiene, que se rellena automáticamente.

Para agilizar el uso de corrutinas (parte de los MonoBehaviour, que las acciones no son), se puede obviar la llamada al agente (agent.StartCoroutine()) y escribir directamente StartCoroutine().

Para hacer que una acción sea válida como acción base, existe un método SetDefaultValues(), que hace todos los cambios necesarios. Es importante que esto no asigna la acción como base de su agente, solo la hace compatible.

## 8. Vídeo
<a href="https://youtu.be/WsstgAS51DI">Escena Maestro de Almas</a>

## 9. Errores
En la documentación original indiqué la intención de integrar los sistemas de percepción de Unity en la herramienta. Esto fue un malentendido por mi parte, pues resulta que Unity no tiene un sistema de percepción, es necesario instalar extensiones.

## 10. Bibliografía

Max Schulz, unity player movement script 3d: https://iqcode.com/code/csharp/unity-player-movement-script-3d
Wello Soft, 10 Skyboxes Pack : Day - Night: https://assetstore.unity.com/packages/2d/textures-materials/sky/10-skyboxes-pack-day-night-32236

