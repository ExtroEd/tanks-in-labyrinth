using System.Windows.Threading;

namespace Client.Logic;

public class RoundManager
{
    private readonly DispatcherTimer _roundTimer;
    private readonly Action _onRoundEnd;
    private bool _isTimerRunning;

    public RoundManager(Action onRoundEnd)
    {
        _onRoundEnd = onRoundEnd;
        _roundTimer = new DispatcherTimer();
        _roundTimer.Tick += (_, _) => EndRound();
    }

    public void CheckRoundCondition()
    {
        var aliveTanks = TankRegistry.Tanks.Count(t => t.IsAlive);

        switch (aliveTanks)
        {
            case 1 when !_isTimerRunning:
                StartCountdown(3);
                break;
            case 0:
                StartCountdown(2);
                break;
        }
    }

    private void StartCountdown(double seconds)
    {
        _roundTimer.Stop();
        _roundTimer.Interval = TimeSpan.FromSeconds(seconds);
        _roundTimer.Start();
        _isTimerRunning = true;
    }
    
    private void EndRound()
    {
        _roundTimer.Stop();
        _isTimerRunning = false;
        _onRoundEnd.Invoke();
    }
    
    public void StopTimer()
    {
        _roundTimer.Stop();
        _isTimerRunning = false;
    }
}
