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
        /// <summary>
        /// Выбор узла для следующего шага.
        /// </summary>
        /// <returns>[0] - сдвиг вправо, [-1] - сдвиг вправо и вверх, [1] - сдвиг вправо и вниз.</returns>
        private int RandomAngle()
        {
            var rr = _rnd.NextDouble();
            if (0 <= rr && rr < 0.33) return -1;
            if (rr <= 0.33 && rr < 0.66) return 0;
            return 1;
        }
        #endregion

        #region Функции, необходимые для расчёта одного МКШ
        /// <summary>
        /// Сделать первый шаг.
        /// </summary>
        /// <param name="par">Атом, для которого требуется сделать шаг.</param>
        /*private void DoFirstStep(Particle par)
        {
            if (_gridStatus[par.X + 1, par.Y]) return; //узел занят!
            
            _gridStatus[++par.X, par.Y] = true; // делаем шаг вправо текущим атомом
            _particles.Add(new Particle(0,par.Y,_diametr)); // просто генерируем ещё один атом на прошлое место (неограниченный источник)
        }*/

        /// <summary>
        /// Сделать любой другой шаг (кроме первого).
        /// </summary>
        /// <param name="par">Атом, для которого требуется сделать шаг.</param>
        private void DoOtherStep(Particle par, bool isFirstStep = false)
        {
            var curY = isFirstStep ? par.Y : SmartY(par.Y + RandomAngle());

            if (_gridStatus[par.X + 1, curY]) return; //узел занят!
            
            _gridStatus[par.X, par.Y] = false;
                
            par.X++;
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
        public void InitAll(int maxY, int maxX, int initPeriod,double diameter)
        {
            _maxY = maxY;
            _maxX = maxX;
            _initPeriod = initPeriod;
            _diametr = diameter;
            
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

            for (var y = 0; y < _maxY; y += 1 + _initPeriod)
            {
                _particles.Add(new Particle(0,y,_diametr));
                _gridStatus[0, y] = true;
            }
        }
        /// <summary>
        /// Сделать один Монте-Карло шаг.
        /// </summary>
        private void DoMCS()
        {
            foreach (var par in _particles)
            {
                if (!(_rnd.NextDouble() >= 0.75)) continue; //вероятность 1/4 сделать шаг
                DoOtherStep(par, par.X == 0);
                if (par.X == _maxX - 1) _rightBorder = true; //атом достиг правой границы (т.е. это последний МКШ)
            }
            
            /*for (var y = 0; y < _maxY; y += 1 + _initPeriod)
            {
                if (_gridStatus[0, y]) continue;
                _particles.Add(new Particle(0, y, _diametr));
                _gridStatus[0, y] = true;
            }*/
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