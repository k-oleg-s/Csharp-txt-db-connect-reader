using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace ConsoleApp1
{

    class Program
    {

 static string errorsinfo = "";  // сюда запишем ошибки формата представления данных - выводятся на консоль
 static string errorsconnect = "";  // сюда запишем ошибки формата connect - записываются в файл bad_data
 static List<DBC> dbcs = new List<DBC> { };  // список объектов подключения к базе данных


         static  int Main(string[] args)
        {
            //  вводные данные.  можно поменять.
            string dirandfilename = args[0];          //   Console.WriteLine(dirandfilename);
            string savetodirectory = Directory.GetCurrentDirectory();    //•	Результирующие данные могут быть сохранены в другое местоположение
            Encoding fileencoding = Encoding.UTF8;   // кодировка. утф8 и так по уумолчанию.
            int parts = 5;              // 	Количество частей, на которые разбиваются данные

           

                openfile(dirandfilename, fileencoding );        // Открыли прочитали файл
                validateall(savetodirectory);                   //•	После чтения данных требуется проверить 
                savetofile(parts, savetodirectory);             // правильные данные делим и пишем в файлы



            Console.ReadKey(); 
            if (errorsinfo.Length > 1)   { Console.WriteLine(errorsinfo); Console.ReadKey(); return -1; } // info from check3
            else return 0;
            
        }




     static void openfile(string fn, Encoding fe) 
        {
            using (StreamReader sr = new StreamReader(fn, fe))
            {
                string input = null;
                DBC a = null;
                int linecounter = 0;

                while ((input = sr.ReadLine()) != null) //  При чтении данных из файла требуется проверять формат представления данных
                {
                    // Console.WriteLine(input);
                    input = input.Trim();
                    linecounter++;
                    int p1=0, p2=0, p3=0, p4=0;

                    check1();
                    check2();
                    check3();

                    void check1()
                    {
                     if (input.StartsWith("[") && input.EndsWith("]"))
                        { a = new DBC(input.Replace("[", "").Replace("]", ""));
                            dbcs.Add(a);
                            //Console.WriteLine("ch1 "+input);
                            //Console.WriteLine("ch1 a " + a.header );
                        }
                    }

                    void check2()
                    {
                        //Console.WriteLine("ch2  enter inp " + input);
                        //Console.WriteLine(a != null);
                        //Console.WriteLine("file"+input.Contains("File ="));
                        //Console.WriteLine(input.StartsWith("Connect="));
                        //Console.WriteLine(input.Contains("Srvr ="));
                        if (a != null && input.StartsWith("Connect=") && input.Contains("File=") )
                        {
                            // p1 = input.IndexOf("=")+1;
                            a.connect = input; //.Substring(p1);

                            p1 = input.IndexOf("File=") + 6;
                            p2 = input.IndexOf(";");
                            a.path = input.Substring(p1, p2-p1-1);
                            //Console.WriteLine("ch2 file  " + a.connect+"   "+ a.path);
                        }
                        if (a != null && input.StartsWith("Connect=") && input.Contains("Srvr=") && input.Contains("Ref="))
                        {
                            a.connect = input; //.Substring(p1);
                            p1 = input.IndexOf("Srvr=") + 5;
                            p2 = input.IndexOf(";");                            
                            a.host = input.Substring(p1+1, p2-p1-2); //, input.IndexOf(";")- input.IndexOf("Srvr="));
                                                    
                            p4 = input.IndexOf("Ref=") + 4;
                            p3=input.IndexOf(";",p2+1);
                            a.dbname  = input.Substring(p4+1, p3-p4-2);
                            //Console.WriteLine("ch2 srvr  " + a.host+"   "+ a.dbname);
                        }
                        if (a != null && a.connect == null && input.StartsWith("Connect=")) { a.connect = input; a.isvalid = false; }
                    }
               
                    void check3()   // fills errorsinfo
                    {
                        if (a != null && input.StartsWith("=")) errorsinfo +=  "line:"+linecounter + " отсутствует имя параметра \n";
    //                    if (a != null && !input.Contains("=")) errorsinfo += input + "line:" + linecounter + " отсутствует разделитель имени от значения \n";
                        if (a == null && input != String.Empty) errorsinfo +=  "line:" + linecounter + " отсутствует заголовок данных \n";
                        if (a != null && input == String.Empty)  //  закрываем создание объекта
                        {
                            if (a.connect == null) { errorsinfo +=  "line:" + linecounter +" у объекта "+a.header+ " отсутствует обязательный параметр \n";
                                a.isvalid = false; }                            
                             a = null;//Console.WriteLine("ch3 " + input);
                        }
                    }
                }
            }
        }
       
     static void savetofile(int parts, string todir)
        {
            if (dbcs.Count==0) return;

            string alldata="";
            foreach (DBC v in dbcs)
            {
                if (v.isvalid == true)
                {
                    alldata += "["+v.header+"]\n";
                    alldata += v.connect+"\n\n";
                }
            }

            int start = 0;
            for (int i = 1; i < parts + 1; i++) // здесь алгоритм разделения на 5 частей. т.к. четко критерий разделения не задан, то делю на части равные, в последнюю идет остаток если есть"  start:"+start
            {
                int lngt = alldata.Length/ parts; 
                if (i == parts )  lngt = alldata.Length / (parts) + alldata.Length % (parts); 
                
        //        Console.WriteLine("частей:" + parts+"  длина вся:"+ alldata.Length+ "   part:"+i+ "   part length:"+ lngt+ "  start:" + start);

                string fileanddir = Path.Combine(todir, "base_"+i+".txt");
                using (StreamWriter writer = File.CreateText(fileanddir))
                {
                    writer.Write(alldata.Substring(start, lngt));
                    writer.Write(writer.NewLine);
                }

                start += lngt;

                fileanddir = Path.Combine(todir, "base_all.txt");
                using (StreamWriter writer = File.CreateText(fileanddir))
                {
                    writer.Write(alldata);
                    writer.Write(writer.NewLine);
                }

            }
        }

        static void validateall(string errtodir)
        {
            foreach (DBC z in dbcs)
            {
                //Console.WriteLine("header:" + z.header + "  connect:" + z.connect + "  path:" + z.path + "  srvr:" + z.host + "  dbname:" + z.dbname + "  isvalid:" + z.isvalid);
                if (z.isvalid == false)
                    errorsconnect += "[" + z.header + "]    ошибка в параметре connect  \n" + z.connect + "\n\n";
                else
                {
                    if (z.path != null) // проверка на некорректные символы в пути
                    {
                        char[] invchars = Path.GetInvalidPathChars();
                        foreach (char ch in invchars)
                        {
                            //Console.WriteLine(" invalid char:" + ch + "\n");
                            //   errorsconnect += "inv:" + ch;
                            if (z.path.Contains(ch))
                            {
                                errorsconnect += "[" + z.header + "]    " + " path has invalid char:" + ch + "\n " + z.connect + "\n\n";
                                z.isvalid = false;
                            }
                        }
                        //errorsconnect += "\n";
                    }

                    if (z.connect.Contains("Srvr=") || z.connect.Contains("Ref="))
                    {
                        if (z.host == null || z.dbname == null || z.host.Length == 0 || z.dbname.Length == 0)
                        {
                            errorsconnect += "[" + z.header + "]    " + " hasnt host or dbname " + "\n " + z.connect + "\n";
                            z.isvalid = false;
                        }
                    }


                }
            }

            if (errorsconnect.Length > 1) // если есть ошибки в поле connect, пишем их в файл
            {
                string f1 = Path.Combine(errtodir, "bad_data.txt");
                using (StreamWriter sr = File.CreateText(f1))
                { sr.Write(errorsconnect); }
            }
        }
    }






}

