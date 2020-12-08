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
        private double _margin;

        /// <summary>
        /// X координата ячейки.
        /// </summary>
        public int X
        {
            get => _x;
            set
            {
                _x = value;
                Xcanvas = _x - _margin;
                OnPropertyChanged(nameof(Xcanvas));
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
                Ycanvas = _y - _margin;
                OnPropertyChanged(nameof(Ycanvas));
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Отцентрированная координата X центра частицы для построения на канвасе.
        /// </summary>
        public double Xcanvas { get; set; }
        /// <summary>
        /// Отцентрированная координата Y центра частицы для построения на канвасе.
        /// </summary>
        public double Ycanvas { get; set; }

        /// <summary>
        /// Диаметр (размер) частицы. Одинаковый для всех частиц.
        /// </summary>
        public double Diameter { get; set; } = 0.7;
        

        /// <summary>
        /// Конструктор: явная инициализация координат ячейки.
        /// </summary>
        public Particle(int x, int y,double diametr)
        {
            X = x;
            Y = y;
            Diameter = diametr;
            _margin = Diameter / 2.0;
            
            Xcanvas = X - _margin;
            Ycanvas = Y - _margin;
            
            //Xcanvas = X;
            //Ycanvas = Y;
        }
    }
}