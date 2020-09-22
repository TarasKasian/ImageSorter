using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp6
{
    internal class StaticticsInfo
    {
        private volatile int _filesGeneral = 0;

        private volatile int _filesProcessed = 0;

        private DateTime _operationStartTime;

        private DateTime _operationEndTime;

        private bool _isOperationCompleted = false;

        public int FilesGeneral 
        {
            get => _filesGeneral;
            set 
            {
                _filesGeneral = value;
                OnStatisticsChanged(this);
            }
        }

        public int FilesProcessed 
        {
            get => _filesProcessed;
            set 
            {
                _filesProcessed = value;
                OnStatisticsChanged(this);
            }
        }

        public bool IsOperationCompleted 
        {
            get => _isOperationCompleted;
            set 
            {
                _isOperationCompleted = value;

                if (IsOperationCompleted)
                {
                    _operationEndTime = DateTime.Now;

                    OnOperationCompleted(this);
                }
            }
        }

        public TimeSpan OperationDuration 
        {
            get
            {
                if (IsOperationCompleted)
                    return _operationEndTime - _operationStartTime;

                return TimeSpan.Zero;
            }
        }

        public int FilesLeft => _filesGeneral - _filesProcessed;

        public event Action<StaticticsInfo> StatisticsChanged;

        public event Action<StaticticsInfo> OperationCompleted;

        public StaticticsInfo() 
        {
            _operationStartTime = DateTime.Now;
        }

        private void OnStatisticsChanged(StaticticsInfo staticticsInfo)
            => StatisticsChanged?.Invoke(staticticsInfo);

        private void OnOperationCompleted(StaticticsInfo staticticsInfo)
           => OperationCompleted?.Invoke(staticticsInfo);
    }
}
