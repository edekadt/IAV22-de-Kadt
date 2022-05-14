# IAV22-de-Kadt
Proyecto final individual - Emile de Kadt
## Índice

1. [Introducción](#1-introducci%C3%B3n)

2. [Punto de partida](#2-punto-de-partida)

3. [Proceso](#3-proceso)

    1. [Agent.h](#agent-h)

4. [Restricciones](#4-restricciones)

5. [Planteamiento](#5-planteaminto)

6. [Video](#6-vídeo-demostración)

7. [Errores](#7-errores)

## 1. Introducción
El objetivo de este proyecto es construir una clase "Agente" alternativa a la presentada en la <a href="https://github.com/IAV22-G09/P1">práctica 1 (El Flautista de Hamelin)</a>. Esta versión del agente está pensada para actores con varias acciones breves, varias de ellas con tiempos de recarga, todas con acceso al mismo juego de percepción sensorial. 

## 2. Punto de partida
Este proyecto se basa en una versión primitiva del mismo concepto, en c++, que creé para el motor de juegos <a href="https://github.com/Triturados/Motor">LOVE</a>. Tras haber visto algunas de las limitaciones de esa primera versión (dificultad de uso, elementos poco intuitivos) pondré un mayor enfoque en la usabilidad de esta nueva versión.

El funcionamiento de esta clase se basa en una clase Agent (que era un componente del motor) y su clase interna Action. Agent hace de máquina de estados del actor, abstrayendo la creación y destrucción de las acciones y reproduciendo siempre la que corresponde. Las acciones tienen varias formas de uso, como microcomponentes en su propio derecho. Principalmente se utilizan sobrecargando sus métodos virtuales OnActionStart, ActiveUpdate, PassiveUpdate y ConditionsFulfilled. OnActionStart contiene código que se ejecuta cada vez que se inicia la acción; ActiveUpdate contiene el código que se ejecutará cada frame mientras la acción se esté realizando; y PassiveUpdate contiene código que se ejecutará cada frame, se esté realizando o no la acción (esto se usa principalmente para controlar la prioridad de la acción). Por último, ConditionsFulfilled contiene las condiciones booleanas que necesitan cumplirse para que la acción se comience.

### Código original en c++
### Agent.h <a name="agent-h"></a>
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

## 3. Proceso

### TAREAS:
1. Adaptar el código original a Unity y C#.
2. Mejorar la usabilidad y el encapsulamiento de la clase.
3. Crear interfaz de usuario simple y comprensible.
4. Documentar modo de uso.
4. Integrar soporte para algunas funcionalidades independientes de Unity.
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

## 6. Video
## 7. Errores
## 8. Bibliografia

