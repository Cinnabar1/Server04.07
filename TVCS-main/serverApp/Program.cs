using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Data.SqlClient;

namespace serverApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string connectionString = "";
                // если строка в настройках пустая, то дефолт, иначе из настроек
                if (Properties.Settings.Default.connectionString == "")
                { connectionString = "Server=.\\SQLEXPRESS;Database=Test;Trusted_Connection=True;"; }
                else
                {
                    connectionString = Properties.Settings.Default.connectionString;
                }

                // проверяем, есть ли у нас данные по последним ID, если нет, то берем последнюю запись в базе.

                if (Properties.Settings.Default.PMServiceLogs_id == "")
                {
                    string expr = "SELECT TOP 1 * FROM PMServiceLogs ORDER BY ID DESC";
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            Console.WriteLine("Подключение открыто");
                            SqlCommand command = new SqlCommand(expr, connection);
                            SqlDataReader reader = command.ExecuteReader();

                            if (reader.HasRows) // если есть данные
                            {

                                while (reader.Read()) // построчно считываем данные
                                {

                                    Properties.Settings.Default.PMServiceLogs_id = reader[0].ToString();
                                    Properties.Settings.Default.Save();
                                }
                            }

                            reader.Close();

                        }
                        Console.WriteLine("Подключение закрыто...");
                    }
                    catch
                    {
                        Console.WriteLine("Ошибка PMServiceLogs_id");
                    }
                }

                if (Properties.Settings.Default.PMServiceLogs_H_id == "")
                {
                    string expr = "SELECT TOP 1 * FROM PMServiceLogs_H ORDER BY ID DESC";
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            Console.WriteLine("Подключение открыто");
                            SqlCommand command = new SqlCommand(expr, connection);
                            SqlDataReader reader = command.ExecuteReader();

                            if (reader.HasRows) // если есть данные
                            {

                                while (reader.Read()) // построчно считываем данные
                                {

                                    Properties.Settings.Default.PMServiceLogs_H_id = reader[0].ToString();
                                    Properties.Settings.Default.Save();
                                }
                            }

                            reader.Close();

                        }
                        Console.WriteLine("Подключение закрыто...");
                    }
                    catch
                    {
                        Console.WriteLine("Ошибка PMServiceLogs_H_id");
                    }

                }
                if (Properties.Settings.Default.PointMasterLogs_id == "")
                {
                    string expr = "SELECT TOP 1 * FROM PointMasterLogs ORDER BY ID DESC";
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            Console.WriteLine("Подключение открыто");
                            SqlCommand command = new SqlCommand(expr, connection);
                            SqlDataReader reader = command.ExecuteReader();

                            if (reader.HasRows) // если есть данные
                            {

                                while (reader.Read()) // построчно считываем данные
                                {
                                    Properties.Settings.Default.PointMasterLogs_id = reader[0].ToString();
                                    Properties.Settings.Default.Save();
                                }
                            }

                            reader.Close();

                        }
                        Console.WriteLine("Подключение закрыто...");
                    }
                    catch
                    {
                        Console.WriteLine("Ошибка PointMasterLogs_id");
                    }

                }
            }
            catch(Exception x) {
                Console.WriteLine("Ошибка подключения к бд");
            }

            // запускаем задачу по расписанию казалось бы да
            Thread myThread = new Thread(new ThreadStart(EmailScheduler.Start)); //EmailScheduler.Start();
            myThread.Start();
            
            //сокет
            // Устанавливаем для сокета локальную конечную точку
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11000);

            // Создаем сокет Tcp/Ip
            Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Назначаем сокет локальной конечной точке и слушаем входящие сокеты
            try
            {
                sListener.Bind(ipEndPoint);
                sListener.Listen(10);

                // Начинаем слушать соединения
                while (true)
                {
                    Console.WriteLine("Ожидаем соединение через порт {0}", ipEndPoint);

                    // Программа приостанавливается, ожидая входящее соединение
                    Socket handler = sListener.Accept();
                    string data = null;

                    // Мы дождались клиента, пытающегося с нами соединиться

                    byte[] bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);

                    data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                    string reply = "";
                    // если получили 1, то мы получили емайл, если 2, то нам прислали строку подключения, природа сокетов удивительна
                    Console.Write("Полученный текст: " + data + "\n\n");
                    if (data[0] == '1')
                    {
                        Properties.Settings.Default.email = data.Substring(2);
                        Properties.Settings.Default.Save();
                    }
                    else if (data[0] == '2')
                    {
                        Properties.Settings.Default.connectionString = data.Substring(2);
                        Properties.Settings.Default.Save();
                    }
                    else if (data[0] == '3')
                    {
                        reply = "good";
                        byte[] msg = Encoding.UTF8.GetBytes(reply);
                        handler.Send(msg);
                    }
                    else
                    {
                        reply = "Спасибо за запрос в " + data.Length.ToString()
                            + " символов";
                        byte[] msg = Encoding.UTF8.GetBytes(reply);
                        handler.Send(msg);
                    }

                    // Отправляем ответ клиенту\
                    


                    //if (data.IndexOf("<TheEnd>") > -1)
                    //{
                    //    Console.WriteLine("Сервер завершил соединение с клиентом.");
                    //    break;
                    //}

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.ReadLine();
            }
        }
    }
}
