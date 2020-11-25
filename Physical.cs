using System;
using System.Collections.ObjectModel;

namespace MM_MonteCarlo
{
    /// <summary>
    /// Основная физическая логика приложения.
    /// </summary>
    public class Physical
    {
        #region Поля класса и конструктор
        /// <summary>
        /// Количество узлов решетки по оси Y.
        /// </summary>
        private int _maxY;
        /// <summary>
        /// Количество узлов решетки по оси X.
        /// </summary>
        private int _maxX;
        /// <summary>
        /// Через какое количество узлов располагать атомы при начальной инициализации.
        /// </summary>
        private int _initPeriod;
        /// <summary>
        /// Состояние всех узлов решетки.
        /// true - узел занят, false - узел свободен.
        /// Координаты узла представлены следующим образом [x,y].
        /// </summary>
        private bool[,] _gridStatus;
        /// <summary>
        /// Коллекция всех атомов в исследуемой решетке.
        /// </summary>
        private ObservableCollection<Particle> _particles;
        /// <summary>
        /// Внутренний рандом для класса Physical.
        /// </summary>
        private readonly Random _rnd;
        
        /// <summary>
        /// Конструктор: инициализация коллекции атомов и рандома. 
        /// </summary>
        public Physical()
        {
            _rnd = new Random();
            _particles = new ObservableCollection<Particle>();
        }
        #endregion

        #region Вспомогательные функции
        // TODO
        #endregion

        #region Функции, необходимые для расчёта одного МКШ
        // TODO
        #endregion

        /// <summary>
        /// Проинициализировать или обновить все доступные поля класса.
        /// </summary>
        /// <param name="maxY">Количество узлов решетки по оси Y. (минимум 100)</param>
        /// <param name="maxX">Количество узлов решетки по оси X. (величина = ???)</param>
        /// <param name="initPeriod">Через какое количество узлов располагать атомы при начальной инициализации.</param>
        public void InitAll(int maxY, int maxX, int initPeriod)
        {
            _maxY = maxY;
            _maxX = maxX;
            _initPeriod = initPeriod;
            
            GenerateInitState();
        }
        /// <summary>
        /// Расположить атомы в узлах решетки и сохранить статус поля и сохранить их положения в коллекцию. 
        /// </summary>
        private void GenerateInitState()
        {
            _gridStatus = new bool[_maxX,_maxY];
            _particles.Clear();

            for (var y = 0; y < _maxY; y += 1 + _initPeriod)
            {
                _particles.Add(new Particle(0,y));
                _gridStatus[0, y] = true;
            }
        }
        /// <summary>
        /// Сделать один Монте-Карло шаг.
        /// </summary>
        private void DoMCS()
        {
            //TODO
            foreach (var par in _particles)
            {
                if (!(_rnd.NextDouble() >= 0.75)) continue;
                if (par.X + 1 > _maxX)
                {
                    _gridStatus[par.X, par.Y] = false;
                    //_particles.Remove(); //!!!
                }
                _gridStatus[par.X++, par.Y] = false;
                _gridStatus[par.X, par.Y] = true;
            }
        }
        
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<Particle> GetParticlesCollection()
        {
            DoMCS();
            return  _particles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool[,] GetGridStatus() => _gridStatus;
    }
}