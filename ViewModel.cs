using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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

		public ViewModel()
		{
			var _physical = new Physical();
			ObservableCollection<Particle> particles;
			_physical.InitAll(100,500,0);
			for (var i = 0; i < 500; i++)
			{
				particles = _physical.GetParticlesCollection();
				var gridStatus = _physical.GetGridStatus();
			}
		}
	}
}