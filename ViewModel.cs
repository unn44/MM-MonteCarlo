using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Input;
using System.Windows.Threading;
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

		private readonly DispatcherTimer _timer;
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
		
		private ObservableCollection<Particle> _particles;
		public ObservableCollection<Particle> Particles
		{
			get => _particles;

			set
			{
				_particles = value;
				OnPropertyChanged(nameof(Particles));
			}
		}
		
		#region RelayCommands
		public ICommand Generate { get; set; }
		public ICommand Start { get; set; }
		#endregion

		#region UserVars
		public int MaxY { get; set; } = 100;
		public int MaxX { get; set; } = 150;
		public int InitPeriod { get; set; } = 0;
		public int MaxTime { get; set; } = 5000;
		public double Diam { get; set; } = 0.8;
		#endregion

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
			
			_physical = new Physical();
			_timer = new DispatcherTimer {Interval = new TimeSpan(100000)}; // хз пока... мб стоит вынести в UI.
			_timer.Tick += OnTimedEvent;
			
			Generation();
			
			Generate = new RelayCommand(o => Generation());
			Start = new RelayCommand(o => SetTimer());
		}
		
		private void Generation()
		{
			_timerTick = 0;
			CountSteps = $"Количество МКШ: {_timerTick} ";
			if (!StartOrStop) SetTimer();
			
			_physical.InitAll(MaxY, MaxX, InitPeriod,Diam);
			OnPropertyChanged("Diam");
			OnPropertyChanged("MaxY");
			OnPropertyChanged("MaxX");
			Particles = _physical.GetParticlesCollectionInit();
		}

		private void SetTimer()
		{
			if (StartOrStop)
			{ 
				_timer.Start();
				StartOrStop = false;
				OnPropertyChanged(nameof(StopOrStartName));
				OnPropertyChanged(nameof(StartOrStop));
			}
			else
			{
				_timer.Stop();
				StartOrStop = true;
				OnPropertyChanged(nameof(StopOrStartName));
				OnPropertyChanged(nameof(StartOrStop));
			}
		}
		
		private void OnTimedEvent(object source, EventArgs e)
		{
			Particles = _physical.GetParticlesCollection();
			
			CountSteps = $"Количество МКШ: {++_timerTick} ";
			if (_timerTick != MaxTime && !_physical.GetRightBorderStatus()) return;

			//turn off the timer
			_timer.Stop();
			StartOrStop = true;
			OnPropertyChanged(nameof(StopOrStartName));
			OnPropertyChanged(nameof(StartOrStop));
		}
	}
}