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
		public bool DrawMode { get; set; }
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
		public int MaxX { get; set; } = 125;
		public int InitPeriod { get; set; } = 0;
		public int MaxTime { get; set; } = 5000;
		public double Diam { get; set; } = 0.8;
		#endregion

		public ViewModel()
		{
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
			
			DrawMode = MaxY <= 120;
			OnPropertyChanged(nameof(DrawMode));
		}

		private void SetTimer()
		{
			if (StartOrStop)
			{ 
				_timer.Start();
				StartOrStop = false;
				//DrawMode = MaxY <= 120;
				OnPropertyChanged(nameof(StopOrStartName));
				OnPropertyChanged(nameof(StartOrStop));
				OnPropertyChanged(nameof(DrawMode));
			}
			else
			{
				_timer.Stop();
				StartOrStop = true;
				//DrawMode = MaxY <= 120;
				OnPropertyChanged(nameof(StopOrStartName));
				OnPropertyChanged(nameof(StartOrStop));
				OnPropertyChanged(nameof(DrawMode));
			}
		}
		
		private void OnTimedEvent(object source, EventArgs e)
		{
			//todo:disable visualisation
			
			
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