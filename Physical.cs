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
        /// Диаметр одной частицы.
        /// </summary>
        private double _diametr;
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
        /// Включен ли режим одной трети?
        /// </summary>
        private bool _thirdMode;
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
        /// Флаг: атом достиг правой границы решетки.
        /// </summary>
        private bool _rightBorder;
        
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
        /// <summary>
        /// Обеспечивает периодические граничные условия по оси Y.
        /// </summary>
        /// <param name="y">Новая планируемая координата Y.</param>
        /// <returns></returns>
        private int SmartY(int y)
        {
            if (y == -1) return _maxY - 1;
            if (y == _maxY) return 0;
            return y;
        }
        #endregion

        #region Функции, необходимые для расчёта одного МКШ
        /// <summary>
        /// Сделать шаг для одного атома.
        /// </summary>
        /// <param name="par">Атом, для которого требуется сделать шаг.</param>
        /// <param name="isFirstStep">Если первый шаг, то атом может двигаться только вправо.</param>
        private void DoAtomStep(Particle par, bool isFirstStep = false)
        {
            int curX, curY;
            while (true)
            {
                curX = isFirstStep ? par.X + 1 : par.X + _rnd.Next(-1, 2); 
                curY = isFirstStep ? par.Y : SmartY(par.Y + _rnd.Next(-1, 2));
                if (curX != par.X || curY != par.Y) break; //защита от стояния на месте
            }

            if (_gridStatus[curX, curY]) return; //узел занят!
            
            _gridStatus[par.X, par.Y] = false;
                
            par.X = curX;
            par.Y = curY;
                
            _gridStatus[par.X, par.Y] = true;
        }
        #endregion

        /// <summary>
        /// Проинициализировать или обновить все доступные поля класса.
        /// </summary>
        /// <param name="maxY">Количество узлов решетки по оси Y. (минимум 100)</param>
        /// <param name="maxX">Количество узлов решетки по оси X. (величина = ???)</param>
        /// <param name="initPeriod">Через какое количество узлов располагать атомы при начальной инициализации.</param>
        public void InitAll(int maxY, int maxX, int initPeriod,double diameter, bool thirdMode)
        {
            _maxY = maxY;
            _maxX = maxX;
            _initPeriod = initPeriod;
            _diametr = diameter;
            _thirdMode = thirdMode;
            
            GenerateInitState();
        }
        /// <summary>
        /// Расположить атомы в узлах решетки и сохранить статус поля и сохранить их положения в коллекцию. 
        /// </summary>
        private void GenerateInitState()
        {
            _gridStatus = new bool[_maxX,_maxY];
            _particles.Clear();
            _rightBorder = false;

            if (!_thirdMode)
            {
                for (var y = 0; y < _maxY; y += 1 + _initPeriod)
                {
                    _particles.Add(new Particle(0, y, _diametr));
                    _gridStatus[0, y] = true;
                }
            }
            else
            {
                var deltaY = _maxY / 3;
                for (var y=0; y < deltaY; y++) _gridStatus[0, y] = true;
                for (var y = deltaY; y < deltaY * 2; y++)
                {
                    _particles.Add(new Particle(0, y, _diametr));
                    _gridStatus[0, y] = true;
                }
                for (var y = deltaY * 2; y <_maxY; y++) _gridStatus[0, y] = true;
            }
        }
        /// <summary>
        /// Сделать один Монте-Карло шаг.
        /// </summary>
        private void DoMCS()
        {
            /* движение атомов */
            foreach (var par in _particles)
            {
                if (!(_rnd.NextDouble() >= 0.75)) continue; //вероятность 1/4 сделать шаг
                DoAtomStep(par, par.X == 0);
                if (par.X == _maxX - 1) _rightBorder = true; //атом достиг правой границы (т.е. это последний МКШ)
            }
            
            /* режим неограниченного источника */
            for (var y = 0; y < _maxY; y += 1 + _initPeriod)
            {
                if (_gridStatus[0, y]) continue;
                _particles.Add(new Particle(0, y, _diametr));
                _gridStatus[0, y] = true;
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
        
        public ObservableCollection<Particle> GetParticlesCollectionInit() => _particles;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool[,] GetGridStatus() => _gridStatus;

        public bool GetRightBorderStatus() => _rightBorder;
    }
}