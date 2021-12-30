using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.IO;

namespace lab15
{
    class Program
    {
        static string objlocker = "null";
        private static SemaphoreSlim semaphore = new SemaphoreSlim(2);
        static void Main(string[] args)
        {
            Console.WriteLine("Информация о процессах");
            try
            {
                foreach (Process process in Process.GetProcesses())
                {
                    Console.WriteLine($"ID: {process.Id}, имя процесса: {process.ProcessName}, приоритет: {process.PriorityClass}, " +
                        $"время запуска: {process.StartTime}, выделенный для процесса объем памяти: {process.VirtualMemorySize64}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("Информация о текущем домене приложения");
            AppDomain domain = AppDomain.CurrentDomain;
            Console.WriteLine($"Имя домена - {domain.FriendlyName}, детали конфигурации - {domain.SetupInformation.ApplicationBase}, {domain.SetupInformation.ConfigurationFile}");
            Console.WriteLine("Все сборки, загруженные в домен:");
            Assembly[] assemblies = domain.GetAssemblies();
            foreach (Assembly asm in assemblies)
                Console.WriteLine(asm.GetName().Name);

            Console.WriteLine("Создание нового домена");
            AppDomain newDomain = AppDomain.CreateDomain("NewDomain");
            /*newDomain.Load(@"D:\C#\lab14\lab14\bin\Debug\System.Buffers.dll");
            AppDomain.Unload(domain);*/

            // Вывод чисел отдельным потоком по порядку 

            Console.WriteLine("Введите n: ");
            int num = Convert.ToInt32(Console.ReadLine());
            Thread myThread = new Thread(Count);
            myThread.Start(num);
            Thread thread = Thread.CurrentThread;
            thread.Name = "Count thread";
            Console.WriteLine($"Имя потока: {thread.Name}, статус: {thread.ThreadState}, приоритет: {thread.Priority}");

            // Вывод чисел двумя потоками (сначала четные, затем нечетные)

            Console.WriteLine("Введите n: ");
            int n = Convert.ToInt32(Console.ReadLine());
            Thread thread1 = new Thread(CountOdd);
            Thread thread2 = new Thread(CountEven);
            thread2.Priority = ThreadPriority.Highest;
            thread2.Start(n);
            thread1.Start(n);

            // Вывод чисел двумя потоками (по порядку с использованием семафора)

            Console.WriteLine("Введите n: ");
            int nu = Convert.ToInt32(Console.ReadLine());
            Thread thread1sync = new Thread(CountEvenSync);
            Thread thread2sync = new Thread(CountOddSync);
            thread1sync.Start(nu);
            thread2sync.Start(nu);

            // Повторяющаяся задача

            TimerCallback tm = new TimerCallback(Message);  // объект делегата, принимающий метод
            // создание таймера(объект делегата, значение передаваемое в метод, кол-во милиисекунд, через которое запуститься таймер, интервал)
            Timer timer = new Timer(tm, null, 0, 2000);

            Console.Read();
        }

        public static void Count(object n)
        {
            int num = (int)n;
            using (StreamWriter sw = new StreamWriter(@"D:\C#\lab15\lab15\file.txt", false))
            {
                for (int i = 1; i < num + 1; i++)
                {
                    Console.WriteLine(i);
                    sw.WriteLine(i);
                    Thread.Sleep(100);
                }
            }
        }
        public static void CountEven(object n)
        {
            int num = (int)n;
            lock (objlocker)
            {
                using (StreamWriter sw = new StreamWriter(@"D:\C#\lab15\lab15\file1.txt", true))
                {
                    for (int i = 1; i < num + 1; i++)
                    {
                        if (i % 2 == 0)
                        {
                            Console.WriteLine(i);
                            sw.WriteLine(i);
                        }
                        Thread.Sleep(400);
                    }
                }
            }

        }
        public static void CountOdd(object n)
        {
            int num = (int)n;
            lock (objlocker)
            {
                using (StreamWriter sw = new StreamWriter(@"D:\C#\lab15\lab15\file1.txt", true))
                {
                    for (int i = 1; i < num + 1; i++)
                    {
                        if (i % 2 == 1)
                        {
                            Console.WriteLine(i);
                            sw.WriteLine(i);
                        }
                        Thread.Sleep(200);
                    }
                }
            }
        }
        public static void CountEvenSync(object n)
        {
            int num = (int)n;
            for (int i = 0; i < num + 1; i += 2)
            {
                semaphore.Wait();
                Console.WriteLine(i);
                Thread.Sleep(190);
                semaphore.Release();
            }

        }
        public static void CountOddSync(object n)
        {
            int num = (int)n;
            for (int i = 1; i < num + 1; i += 2)
            {
                semaphore.Wait();
                Console.WriteLine(i);
                Thread.Sleep(200);
                semaphore.Release();
            }

        }
        public static void Message(object obj)
        {
            Console.WriteLine("Message.");
        }
    }
}