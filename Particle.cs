using System.ComponentModel;
using System.Runtime.CompilerServices;
using MM_MonteCarlo.Annotations;

namespace MM_MonteCarlo
{
    /// <summary>
    /// Информация об одной частице: координаты ячейки, где она находится.
    /// </summary>
    public class Particle : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _x, _y;

        /// <summary>
        /// X координата ячейки.
        /// </summary>
        public int X
        {
            get => _x;
            set
            {
                _x = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Y координата ячейки.
        /// </summary>
        public int Y
        {
            get => _y;
            set
            {
                _y = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Диаметр (размер) частицы. Одинаковый для всех частиц.
        /// </summary>
        public double Diameter { get; set; }
        

        /// <summary>
        /// Конструктор: явная инициализация координат ячейки.
        /// </summary>
        public Particle(int x, int y,double diametr)
        {
            X = x;
            Y = y;
            Diameter = diametr;
        }
    }
}