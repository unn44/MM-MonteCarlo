using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Input;
using System.Windows.Threading;
using MM_MonteCarlo.Annotations;
using OxyPlot;

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

		private int _t1Tick, _t2Tick, _t3Tick, _t4Tick; 

		private Physical _physical;
		
		public bool StartOrStop { get; set; } = true;
		public bool DrawMode { get; set; }
		public string StopOrStartName => StartOrStop ? "Запустить" : "Приостановить";
		
		private string _countSteps, _infoTimeI;
		public string CountSteps
		{
			get => _countSteps;
			set
			{
				_countSteps = value;
				OnPropertyChanged();
			}
		}
		
		public string InfoTimeI
		{
			get => _infoTimeI;
			set
			{
				_infoTimeI = value;
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

		#region Charts

		private int _invalidateFlagExp, _invalidateFlagTheory;
		public int InvalidateFlagExp
		{
			get => _invalidateFlagExp;
			set
			{
				_invalidateFlagExp = value;
				OnPropertyChanged();
			}
		}
		public int InvalidateFlagTheory
		{
			get => _invalidateFlagTheory;
			set
			{
				_invalidateFlagTheory = value;
				OnPropertyChanged();
			}
		}
		
		public List<DataPoint> PointsT1exp { get; set; }
		public List<DataPoint> PointsT2exp { get; set; }
		public List<DataPoint> PointsT3exp { get; set; }
		public List<DataPoint> PointsT4exp { get; set; }
		
		public List<DataPoint> PointsT1theory { get; set; }
		public List<DataPoint> PointsT2theory { get; set; }
		public List<DataPoint> PointsT3theory { get; set; }
		public List<DataPoint> PointsT4theory { get; set; }
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

			PointsT1exp = new List<DataPoint>();
			PointsT2exp = new List<DataPoint>();
			PointsT3exp = new List<DataPoint>();
			PointsT4exp = new List<DataPoint>();
			
			PointsT1theory = new List<DataPoint>();
			PointsT2theory = new List<DataPoint>();
			PointsT3theory = new List<DataPoint>();
			PointsT4theory = new List<DataPoint>();
			
			Generation();
			
			Generate = new RelayCommand(o => Generation());
			Start = new RelayCommand(o => SetTimer());
		}
		
		private void Generation()
		{
			_timerTick = 0;
			CountSteps = $"Количество МКШ: {_timerTick} ";
			InfoTimeI = "";
			if (!StartOrStop) SetTimer();
			
			_physical.InitAll(MaxY, MaxX, InitPeriod,Diam);
			OnPropertyChanged("Diam");
			OnPropertyChanged("MaxY");
			OnPropertyChanged("MaxX");
			Particles = _physical.GetParticlesCollectionInit();
			
			DrawMode = MaxY <= 120;
			OnPropertyChanged(nameof(DrawMode));

			InvalidateFlagExp = 0;
			PointsT1exp.Clear();
			PointsT2exp.Clear();
			PointsT3exp.Clear();
			PointsT4exp.Clear();
		}

		private void SetTimer()
		{
			_t1Tick = (int)Math.Round(MaxTime / 4.0);
			_t2Tick = _t1Tick * 2;
			_t3Tick = _t1Tick * 3;
			_t4Tick = MaxTime;
			
			InfoTimeI = $"tMax = {MaxTime} МКШ\n\nt1 = {_t1Tick} МКШ\nt2 = {_t2Tick} МКШ\nt3 = {_t3Tick} МКШ\nt4 = {_t4Tick} МКШ\n";

			if (StartOrStop)
			{ 
				_timer.Start();
				StartOrStop = false;
				//DrawMode = MaxY <= 120;
			}
			else
			{
				_timer.Stop();
				StartOrStop = true;
				//DrawMode = MaxY <= 120;
			}
			
			OnPropertyChanged(nameof(StopOrStartName));
			OnPropertyChanged(nameof(StartOrStop));
			OnPropertyChanged(nameof(DrawMode));
		}
		
		private void OnTimedEvent(object source, EventArgs e)
		{
			Particles = _physical.GetParticlesCollection();
			
			CountSteps = $"Количество МКШ: {++_timerTick} ";

			if (_timerTick == _t1Tick) DrawImpurityDistribution(PointsT1exp);
			if (_timerTick == _t2Tick) DrawImpurityDistribution(PointsT2exp);
			if (_timerTick == _t3Tick) DrawImpurityDistribution(PointsT3exp);
			if (_timerTick == _t4Tick) DrawImpurityDistribution(PointsT4exp);
			
			if (_timerTick != MaxTime && !_physical.GetRightBorderStatus()) return;
			
			//turn off the timer
			_timer.Stop();
			StartOrStop = true;
			OnPropertyChanged(nameof(StopOrStartName));
			OnPropertyChanged(nameof(StartOrStop));
		}

		private void DrawImpurityDistribution(List<DataPoint> points)
		{
			var gridStat = _physical.GetGridStatus();
			for (var i = 0; i < MaxX; i++)
			{
				var atoms = 0;
				for (var j = 0; j < MaxY; j++)
				{
					if (gridStat[i, j]) atoms++;
				}
				InvalidateFlagExp++;
				points.Add(new DataPoint(i, atoms));
			}
		}
	}
}