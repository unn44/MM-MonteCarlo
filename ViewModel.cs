using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Input;
using MM_MonteCarlo.Annotations;

namespace MM_MonteCarlo
{
public class ViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		
		private readonly Timer _timer;
		private int _timerTick;

		private Physical _physical;
		
		public bool StartOrStop { get; set; } = true;
		public string StopOrStartName => StartOrStop ? "Запустить" : "Приостановить";
		
		private string _countSteps;
		public string CountSteps
		{
			get => _countSteps;
			set
			{
				_countSteps = value;
				OnPropertyChanged();
			}
		}
		
		#region RelayCommands
		public ICommand Generate { get; set; }
		public ICommand Start { get; set; }
		#endregion

		#region UserVars
		public int MaxY { get; set; } = 100;
		public int MaxX { get; set; } = 250;
		public int InitPeriod { get; set; } = 0;
		public int MaxTime { get; set; } = 100;
		#endregion

		private void Generation()
		{
			_timerTick = 0;
			CountSteps = $"Количество МКШ: {_timerTick} ";
			if (!StartOrStop) SetTimer();
			
			//TODO: Physical init
		}
		
		public ViewModel()
		{
			/*var _physical = new Physical();
			ObservableCollection<Particle> particles;
			_physical.InitAll(100,500,0);
			for (var i = 0; i < 500; i++)
			{
				particles = _physical.GetParticlesCollection();
				var gridStatus = _physical.GetGridStatus();
			}*/
			
			_timer = new Timer(100); // хз пока...
			_timer.Elapsed += OnTimedEvent;
			
			Generation();
			
			Generate = new RelayCommand(o => Generation());
			Start = new RelayCommand(o => SetTimer());
		}

		private void SetTimer()
		{
			if (StartOrStop)
			{
				_timer.AutoReset = true;
				_timer.Enabled = true;
				StartOrStop = false;
				OnPropertyChanged(nameof(StopOrStartName));
				OnPropertyChanged(nameof(StartOrStop));
			}
			else
			{
				_timer.Enabled = false;
				StartOrStop = true;
				OnPropertyChanged(nameof(StopOrStartName));
				OnPropertyChanged(nameof(StartOrStop));
			}
		}
		
		private void OnTimedEvent(object source, ElapsedEventArgs e)
		{
			//TODO: all actions
			
			CountSteps = $"Количество МКШ: {++_timerTick} ";
			if (_timerTick != MaxTime) return; //TODO: right border check
			
			//turn off the timer
			_timer.Enabled = false;
			StartOrStop = true;
			OnPropertyChanged(nameof(StopOrStartName));
			OnPropertyChanged(nameof(StartOrStop));
		}
	}
}