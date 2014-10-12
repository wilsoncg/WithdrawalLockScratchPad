using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CityIndex.Atlas.Data;

namespace WithdrawalLockScratchPad
{
    class Program
    {
        private static int _clientAccountId = 10;
        private static CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private static Task _task;
        private static object _lock = new object();
        private static ManualResetEvent _resetEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            Console.WriteLine("Press Escape (Esc) key to quit. {0}", Environment.NewLine);
            Console.WriteLine("(S)tart thread");
            ConsoleKeyInfo cki;
            Console.TreatControlCAsInput = true;
            do
            {
                cki = Console.ReadKey(true);
                if (cki.Key == ConsoleKey.S)
                {
                    if (_task != null && _task.Status == TaskStatus.Running)
                    {
                        _resetEvent.Reset();
                    }
                    else
                    {
                        RunTask();
                        _resetEvent.Set();
                    }
                }
                if (cki.Key == ConsoleKey.Escape)
                {
                    CancelTask();
                    break;
                }
            } while (true);
        }

        private static void CancelTask()
        {
            Console.WriteLine("Stopping Thread {0}", Thread.CurrentThread.ManagedThreadId);
            _cancellationToken.Cancel();
        }

        private static void RunTask()
        {
            _task = new TaskFactory(_cancellationToken.Token).StartNew(() =>
            {
                do
                {
                    if (_cancellationToken.Token.IsCancellationRequested)
                        break;

                    _resetEvent.WaitOne();

                    var threadId = Thread.CurrentThread.ManagedThreadId;
                    var locked = ClientAccountDA.LockClientAccount(_clientAccountId);
                    Print(threadId, _clientAccountId, locked);
                    locked = ClientAccountDA.UnlockClientAccount(_clientAccountId);
                    Print(threadId, _clientAccountId, locked);
                } while (true);
            });
        }

        private static void Print(int threadId, int clientAccountId, bool isLocked)
        {
            Console.WriteLine("Thread {0}; ClientAccount {1} isLocked={2}", threadId, clientAccountId, isLocked);
        }
    }

    //public class TaskStatusObserver : IObservable<Task>
    //{
    //    public IDisposable Subscribe(IObserver<Task> observer)
    //    {
            
    //    }
    //}
}
