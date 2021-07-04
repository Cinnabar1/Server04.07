using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using System.Net;
using System.Net.Mail;
using System.Data.SqlClient;

namespace serverApp
{
    public class EmailSender : IJob
    {
        // сюда пишутся из бд строки
        List<string[]> data1 = new List<string[]>();
        List<string[]> data2 = new List<string[]>();

        public async Task Execute(IJobExecutionContext context)


        {
            data1.Clear();
            data2.Clear();
            // удалить 1 строку
            string connectionString = "";
            if (Properties.Settings.Default.connectionString == "")
                { connectionString = "Server=.\\SQLEXPRESS;Database=Test;Trusted_Connection=True;"; }
            else
            {
                connectionString = Properties.Settings.Default.connectionString;
            }
            
            

            string sqlExpression = String.Format("SELECT * FROM PMServiceLogs WHERE ErrStatus = 1 AND ID > {0} ORDER BY ID", Properties.Settings.Default.PMServiceLogs_id);
            string sqlExpression1 = String.Format("SELECT * FROM PMServiceLogs_H WHERE ErrStatus = 1 AND ID > {0} ORDER BY ID", Properties.Settings.Default.PMServiceLogs_H_id);
            string sqlExpression2 = String.Format("SELECT * FROM PointMasterLogs WHERE (Msg LIKE '%Error%' OR Msg LIKE '%error%' OR Msg LIKE '%ERROR%') AND ID > {0} ORDER BY ID", Properties.Settings.Default.PointMasterLogs_id);

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    Console.WriteLine("Подключение открыто");
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    //List<string[]> data1 = new List<string[]>();
                    //List<string[]> data2 = new List<string[]>();

                    if (reader.HasRows) // если есть данные
                    {
                        // выводим названия столбцов
                        //Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}", reader.GetName(0), reader.GetName(1), reader.GetName(2), reader.GetName(3), reader.GetName(4), reader.GetName(5), reader.GetName(6), reader.GetName(7), reader.GetName(8));

                        while (reader.Read()) // построчно считываем данные
                        {
                            data1.Add(new string[10]);
                            data1[data1.Count - 1][0] = reader[0].ToString();
                            data1[data1.Count - 1][1] = " ";
                            data1[data1.Count - 1][2] = reader[1].ToString();
                            data1[data1.Count - 1][3] = reader[2].ToString();
                            data1[data1.Count - 1][4] = reader[3].ToString();
                            data1[data1.Count - 1][5] = reader[4].ToString();
                            data1[data1.Count - 1][6] = reader[5].ToString();
                            data1[data1.Count - 1][7] = reader[6].ToString();
                            data1[data1.Count - 1][8] = reader[7].ToString();
                            data1[data1.Count - 1][9] = reader[8].ToString();

                        }
                    }

                    reader.Close();
                    // меняем ласт айди таблицы
                    Properties.Settings.Default.PMServiceLogs_id = data1[data1.Count - 1][0];

                    SqlCommand command1 = new SqlCommand(sqlExpression1, connection);
                    SqlDataReader reader1 = command1.ExecuteReader();

                    if (reader1.HasRows) // если есть данные
                    {


                        while (reader1.Read()) // построчно считываем данные
                        {
                            data1.Add(new string[10]);
                            data1[data1.Count - 1][0] = reader1[0].ToString();
                            data1[data1.Count - 1][1] = " ";
                            data1[data1.Count - 1][2] = reader1[1].ToString();
                            data1[data1.Count - 1][3] = reader1[2].ToString();
                            data1[data1.Count - 1][4] = reader1[3].ToString();
                            data1[data1.Count - 1][5] = reader1[4].ToString();
                            data1[data1.Count - 1][6] = reader1[5].ToString();
                            data1[data1.Count - 1][7] = reader1[6].ToString();
                            data1[data1.Count - 1][8] = reader1[7].ToString();
                            data1[data1.Count - 1][9] = reader1[8].ToString();
                        }
                    }

                    reader1.Close();
                    Properties.Settings.Default.PMServiceLogs_H_id = data1[data1.Count - 1][0];

                    SqlCommand command2 = new SqlCommand(sqlExpression2, connection);
                    SqlDataReader reader2 = command2.ExecuteReader();

                    if (reader2.HasRows) // если есть данные
                    {


                        while (reader2.Read()) // построчно считываем данные
                        {
                            data2.Add(new string[10]);
                            data2[data2.Count - 1][0] = reader2[0].ToString();
                            data2[data2.Count - 1][1] = reader2[1].ToString();
                            data2[data2.Count - 1][2] = reader2[5].ToString();
                            data2[data2.Count - 1][3] = reader2[2].ToString();
                            data2[data2.Count - 1][4] = " ";
                            data2[data2.Count - 1][5] = " ";
                            data2[data2.Count - 1][6] = reader2[3].ToString();
                            data2[data2.Count - 1][7] = reader2[4].ToString();
                            data2[data2.Count - 1][8] = " ";
                            data2[data2.Count - 1][9] = reader2[6].ToString();
                            //if (!list.Contains(reader2[4].ToString()))
                            //{
                            //    list.Add(reader2[4].ToString());
                            //}
                        }
                    }

                    reader2.Close();
                    Properties.Settings.Default.PointMasterLogs_id = data1[data1.Count - 1][0];


                    // отправитель - устанавливаем адрес и отображаемое в письме имя
                    MailAddress from = new MailAddress("irinakinovar@mail.ru", "Tom");
                    // кому отправляем
                    MailAddress to = new MailAddress(Properties.Settings.Default.email);
                    // создаем объект сообщения
                    MailMessage m = new MailMessage(from, to);
                    // тема письма
                    m.Subject = "Выявлены новые ошибки";
                    m.Body = "<h5>Подробные данные: <h5><br>";

                    for (int i = 0; i < data1.Count; i++)
                    {
                        //заглавия таблицы
                        m.Body += "<table border=\"1\" cellpadding=\"0\" cellspacing=\"0\" style=\"margin: 0; padding: 0\">"
                            + "<tr><th> ID </th> " + "<th> DateTimeS </th>"
                            + "<th> FileName </th>" + "<th> FileNameDest </th>"
                            + "<th> Operation </th>" + "<th> MngrName </th>" + "<th> MngrID </th>"
                            + "<th> ErrStatus </th>" + "<th> Comment </th></tr><tr>";
                        for (int j = 0; j < 10; j++)
                        {
                            if (data1[i][j] != " ")
                                m.Body += "<td>" + data1[i][j] + "</td>";
                        }
                        m.Body += "</tr></table> <br><br>";
                    }
                    for (int i = 0; i < data2.Count; i++)
                    {
                        m.Body += "<table border=\"1\" cellpadding=\"0\" cellspacing=\"0\" style=\"margin: 0; padding: 0\">"
                           + "<tr><th> ID </th> " + "<th> HarrisID </th>"
                           + "<th> FileName </th>" + "<th> MngrName </th>"
                           + "<th> MngrID </th>" + "<th> DateTimeS </th>"
                           + "<th> Msg </th></tr><tr>";
                        for (int j = 0; j < 10; j++)
                        {
                            if (data2[i][j] != " ")
                                m.Body += "<td>" + data2[i][j] + "</td>";
                        }
                        m.Body += "</tr></table> <br><br>";
                    }
                    //foreach (string[] s in data1)
                    //// текст письма
                    //m.Body += s + "\n";


                    //foreach (string[] s in data2)
                    //    m.Body += s + "\n";

                    // письмо представляет код html
                    m.IsBodyHtml = true;
                    // адрес smtp-сервера и порт, с которого будем отправлять письмо
                    SmtpClient smtp = new SmtpClient("smtp.mail.ru", 587);
                    // логин и пароль
                    smtp.Credentials = new NetworkCredential("irinakinovar@mail.ru", "kodnak56");
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(m);

                }
                Console.WriteLine("Подключение закрыто...");
            }
            catch
            {
                Console.WriteLine("Ошибка при отправке на почту");
            }

        }
    }
}
