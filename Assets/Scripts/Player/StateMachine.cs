/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Período: 2.º cuatrimestre de 2026
 * Materia: Programación 2
 * Programador: Eduardo Ortega
 * Función: Define la interfaz IState y gestiona el estado activo.
 * Al cambiar de estado, ejecuta Exit() en el anterior y Enter() en
 * el nuevo. Cada llamada a Update() se delega al estado activo.
 *********************************************************************************************/
public interface IState
{
    void Enter();
    void Update();
    void Exit();
}

public class StateMachine
{
    public IState CurrentState { get; private set; }

    public void Initialize(IState startingState)
    {
        CurrentState = startingState;
        CurrentState?.Enter();
    }

    public void ChangeState(IState newState)
    {
        if (newState == null || newState == CurrentState) return;

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void Update()
    {
        CurrentState?.Update();
    }
}