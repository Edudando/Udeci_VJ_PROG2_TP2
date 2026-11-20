/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Período: 2.º cuatrimestre de 2026
 * Materia: Programación 2
 * Programador: Eduardo Ortega
 * Función: Implementa los estados de reposo, movimiento y muerte de Nia.
 * Mantiene la gravedad en reposo y alterna entre reposo y movimiento
 * según las teclas de control. Al morir, detiene el movimiento de Nia
 * y, si revive, vuelve al estado de reposo.
 *********************************************************************************************/
using UnityEngine;

// --- ESTADO IDLE (REPOSO) ---
public class PlayerIdleState : IState
{
    private PlayerController3D player;

    public PlayerIdleState(PlayerController3D player)
    {
        this.player = player;
    }

    public void Enter() { }

    public void Update()
    {
        if (!player.IsAlive)
        {
            player.StateMachine.ChangeState(player.DeadState);
            return;
        }

        player.HandleInputs();
        player.ApplyGravityOnly(); // Mantiene la gravedad activa en reposo

        if (player.HasMovementInput())
        {
            player.StateMachine.ChangeState(player.MoveState);
        }
    }

    public void Exit() { }
}

// --- ESTADO MOVE (MOVIMIENTO) ---
public class PlayerMoveState : IState
{
    private PlayerController3D player;

    public PlayerMoveState(PlayerController3D player)
    {
        this.player = player;
    }

    public void Enter() { }

    public void Update()
    {
        if (!player.IsAlive)
        {
            player.StateMachine.ChangeState(player.DeadState);
            return;
        }

        player.HandleInputs();
        player.Move();

        if (!player.HasMovementInput())
        {
            player.StateMachine.ChangeState(player.IdleState);
        }
    }

    public void Exit() { }
}

// --- ESTADO DEAD (MUERTO) ---
public class PlayerDeadState : IState
{
    private PlayerController3D player;

    public PlayerDeadState(PlayerController3D player)
    {
        this.player = player;
    }

    public void Enter()
    {
        player.ResetMotion();
    }

    public void Update()
    {
        // Transición de regreso si revive
        if (player.IsAlive)
        {
            player.StateMachine.ChangeState(player.IdleState);
        }
    }

    public void Exit() { }
}