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

		private Physical _physical;
		
		public bool StartOrStop { get; set; } = true;
		public bool DrawMode { get; set; }
		public bool ThirdMode { get; set; }
		public bool IsTheoryAvailable { get; set; } = false;
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
		
		public double CanvasBorderThickness { get; set; } = 2;
		public double CanvasScale { get; set; } = 8;
		public double CanvasX { get; set; }
		public double CanvasY { get; set; }
		
		#region RelayCommands
		public ICommand Generate { get; set; }
		public ICommand Start { get; set; }
		public ICommand CalcTheory { get; set; }
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
		public int MaxY { get; set; } = 102;
		public int MaxX { get; set; } = 125;
		public int InitPeriod { get; set; } = 0;
		public int MaxTime { get; set; } = 5000;
		public double Diam { get; set; } = 0.8;
		public double C0 { get; set; } = 102;
		public double D { get; set; } = 0.1;
		
		public int T1Tick { get; set; }
		public int T2Tick { get; set; }
		public int T3Tick { get; set; }
		public int T4Tick { get; set; }
		#endregion

		public ViewModel()
		{
			_physical = new Physical();
			_timer = new DispatcherTimer();
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
			CalcTheory = new RelayCommand(o => CalcTheoreticalDistribution());
		}
		
		private void Generation()
		{
			_timerTick = 0;
			CountSteps = $"Количество МКШ: {_timerTick} ";
			InfoTimeI = "";
			if (!StartOrStop) SetTimer();

			ThirdMode = MaxY%3 == 0 && ThirdMode;
			OnPropertyChanged(nameof(ThirdMode));
			if (ThirdMode) InitPeriod = 0;
			OnPropertyChanged(nameof(InitPeriod));
			
			_physical.InitAll(MaxY, MaxX, InitPeriod,Diam, ThirdMode);
			OnPropertyChanged("Diam");
			OnPropertyChanged("MaxY");
			OnPropertyChanged("MaxX");

			CanvasX = MaxX * CanvasScale + CanvasBorderThickness * 2;
			CanvasY = MaxY * CanvasScale + CanvasBorderThickness * 2;
			OnPropertyChanged(nameof(CanvasX));
			OnPropertyChanged(nameof(CanvasY));
			
			Particles = _physical.GetParticlesCollectionInit();
			
			DrawMode = MaxY <= 120;
			OnPropertyChanged(nameof(DrawMode));

			IsTheoryAvailable = false;
			OnPropertyChanged(nameof(IsTheoryAvailable));

			InvalidateFlagExp = 0;
			PointsT1exp.Clear();
			PointsT2exp.Clear();
			PointsT3exp.Clear();
			PointsT4exp.Clear();
			
			T1Tick = (int)Math.Round(MaxTime / 9.0);
			T2Tick = T1Tick * 4;
			T3Tick = MaxTime;

			OnPropertyChanged(nameof(T1Tick));
			OnPropertyChanged(nameof(T2Tick));
			OnPropertyChanged(nameof(T3Tick));
		}

		private void SetTimer()
		{
			_timer.Interval = new TimeSpan(DrawMode ? 100000 : 1);

			OnPropertyChanged(nameof(T1Tick));
			OnPropertyChanged(nameof(T2Tick));
			OnPropertyChanged(nameof(T3Tick));
			
			C0 = MaxY;
			IsTheoryAvailable = true;

			ThirdMode = MaxY%3 == 0 && ThirdMode;
			OnPropertyChanged(nameof(ThirdMode));
			if (ThirdMode) InitPeriod = 0;
			OnPropertyChanged(nameof(InitPeriod));

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
			OnPropertyChanged(nameof(C0));
			OnPropertyChanged(nameof(IsTheoryAvailable));
		}
		
		private void OnTimedEvent(object source, EventArgs e)
		{
			Particles = _physical.GetParticlesCollection();
			
			CountSteps = $"Количество МКШ: {++_timerTick} ";

			if (_timerTick == T1Tick) DrawImpurityDistribution(PointsT1exp);
			if (_timerTick == T2Tick) DrawImpurityDistribution(PointsT2exp);
			if (_timerTick == T3Tick) DrawImpurityDistribution(PointsT3exp);

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

				if (ThirdMode && i == 0) atoms -= MaxY / 3 * 2;
				
				InvalidateFlagExp++;
				points.Add(new DataPoint(i, atoms));
			}
		}

		private void CalcTheoreticalDistribution()
		{
			InvalidateFlagTheory = 0;
			
			PointsT1theory.Clear();
			PointsT2theory.Clear();
			PointsT3theory.Clear();
			PointsT4theory.Clear();
			
			DrawTheoreticalDistribution(PointsT1theory, T1Tick);
			DrawTheoreticalDistribution(PointsT2theory, T2Tick);
			DrawTheoreticalDistribution(PointsT3theory, T3Tick);
		}
		
		private void DrawTheoreticalDistribution(List<DataPoint> points, int time)
		{
			for (var x = 0; x < MaxX; x++)
			{
				var argument = x / (2 * Math.Sqrt(D * time));
				InvalidateFlagTheory++;
				points.Add(new DataPoint(x, TheorDistrib.Cfunc(C0,argument)));
			}
		}
	}
}