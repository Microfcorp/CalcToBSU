using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcDiscret
{
    public class TextZapis
    {
        public static TZ[] Parsing(string Text)
        {
            //A={1,3,7,8} ∩ {1,4,5,7}
            List<TZ> opers = new List<TZ>();

            var Operators = Text.Split(new char[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries).Where(a => !a.Contains(",") && !a.Contains(";") && !a.Contains("=") && !StringIsDigits(a)).Select(a => a.Trim()).Select(a => new Operators(a[0])).ToArray();
            var Mnozestv = Mnozestvo.Parse(Text);

            for (int i = 0; i < Mnozestv.Length; i++)
            {
                opers.Add(Mnozestv[i]);
                if(i+1 < Mnozestv.Length) opers.Add(Operators[i]);
            }

            return opers.ToArray();
        }

        public static Mnozestvo Execute(TZ[] data)
        {
            for (int i = 0; i < data.Length; i += 2)
            {
                if (!(i + 2 < data.Length)) break;

                var o1 = data[i];
                var o2 = data[i + 1];
                var o3 = data[i + 2];

                data[i + 2] = MnozestvoOperators.Operating((o2 as Operators).TypesOperators, o1 as Mnozestvo, o3 as Mnozestvo);
            }

            return data.Last() as Mnozestvo;
        }

        static bool StringIsDigits(string s)
        {
            foreach (var item in s)
            {
                if (!char.IsDigit(item))
                    return false; //если хоть один символ не число, то выкидываешь "ложь"
            }
            return true; //если ни разу не выбило в цикле, значит, все символы - это цифры
        }
    }

    public class Mnozestvo : TZ
    {
        public const char Dopoln = '_';
        public const char Peres = 'N';
        public const char Obed = 'U';
        public const char Semetr = 'S';
        public const char Delen = '/';
        public const char Razn = '\\';
        public const char Ravno = '=';
        public const char Pust = 'O';

        public char Name;
        public int[] Elements;

        public Mnozestvo(char name, int[] elements) : this()
        {
            Name = name;
            Elements = elements;
        }

        public Mnozestvo(char name) : this()
        {
            Name = name;
        }

        public bool IsNull
        {
            get => !Elements.Any();
        }

        public Mnozestvo(){ Types = TypeTZ.Mnozestvo; }

        public static Mnozestvo[] Parse(string text) // A={1,3,7,8} и {1,4,5,7}
        {
            var mnText = text.Split(Peres, Obed, Semetr, Delen, Razn, Dopoln).Select(t => t.Trim()).ToArray();

            List<Mnozestvo> mn = new List<Mnozestvo>();

            foreach (var m1 in mnText)
            {
                Mnozestvo m = new Mnozestvo();
                var rv = m1.Split(new char[2] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
                if (rv.Length == 0)
                    continue;
                else if (rv.Length == 1)
                {
                    m.Elements = rv.FirstOrDefault().Split(',', ';').Select(t => Convert.ToInt32(t)).ToArray();
                    //65
                    m.Name = Convert.ToChar(mn.Any() ? mn.Max(t => ((byte)t.Name)) + 1 : 65);
                }
                else if (rv.Length > 1)
                {
                    m.Elements = rv[1].Split(',', ';').Select(t => Convert.ToInt32(t)).ToArray();
                    m.Name = rv[0].Split('=').FirstOrDefault()[0];
                }
                mn.Add(m);
            }

            return mn.ToArray();
        }

        private string parsing(string s, string param, char delite = ',')
        {
            var dict = s.Split(delite)
        .Select(part => part.Split('=')
                            .Select(token => token.Trim('"')))
        .ToDictionary(tokens => tokens.First(),
                      tokens => tokens.Skip(1).Single());


            var res = dict[param];
            return res;
        }

        public override string ToString()
        {
            return ((Name != '\0') ? Name + "=" : "") + (IsNull ? Pust.ToString() : ("{" + String.Join(",", Elements) + "}"));
        }

        public static Mnozestvo U
        {
            get
            {
                var m = new Mnozestvo('U');

                const int s = 10000000;
                m.Elements = new int[s];
                for (int i = -1000000, u = 0; i <= 10000000 && u < s; i++, u++)
                    m.Elements[u] = i;

                return m;
            }
        }

        public void Sort()
        {
            Elements = Elements.OrderBy(x => x).ToArray();
        }
    }

    public class Operators : TZ
    {
        public MnozestvoOperators.OperatorsType TypesOperators;
        public char Oper;

        public Operators()
        {
            Types = TypeTZ.Operators;
        }

        public Operators(char oper) : this()
        {
            Oper = oper;
            TypesOperators = MnozestvoOperators.Operating(oper);
        }
    }

    public static class MnozestvoOperators
    {
        public enum OperatorsType
        {
            Peres, //∩ 
            Obied, //∪
            //Ravno, //
            Razn, // \
            SRazn, // ∆
            Dopoln, // _
        }

        public static OperatorsType Operating(char operators)
        {
            if (operators == Mnozestvo.Semetr)
                return OperatorsType.SRazn;
            else if (operators == Mnozestvo.Razn)
                return OperatorsType.Razn;
            else if (operators == Mnozestvo.Obed)
                return OperatorsType.Obied;
            else if (operators == Mnozestvo.Peres)
                return OperatorsType.Peres;
            else if (operators == Mnozestvo.Dopoln)
                return OperatorsType.Dopoln;
            else return OperatorsType.Peres;
        }

        public static Mnozestvo Operating(char operators, params Mnozestvo[] mns)
        {
            if (operators == Mnozestvo.Semetr)
                return SRazn(mns);
            else if (operators == Mnozestvo.Razn)
                return Razn(mns);
            else if (operators == Mnozestvo.Obed)
                return Obied(mns);
            else if (operators == Mnozestvo.Peres)
                return Peres(mns);
            else if (operators == Mnozestvo.Dopoln)
                return Dopoln(mns);
            else return new Mnozestvo();
        }

        public static Mnozestvo Operating(OperatorsType operators, params Mnozestvo[] mns)
        {
            if (operators == OperatorsType.SRazn)
                return SRazn(mns);
            else if (operators == OperatorsType.Razn)
                return Razn(mns);
            else if (operators == OperatorsType.Obied)
                return Obied(mns);
            else if (operators == OperatorsType.Peres)
                return Peres(mns);
            else if (operators == OperatorsType.Dopoln)
                return Dopoln(mns);
            else return new Mnozestvo();
        }

        public static Mnozestvo Peres(params Mnozestvo[] mns)
        {
            Mnozestvo mn = new Mnozestvo('Z');

            mn.Elements = GetSeq(mns.Select(t => t.Elements).ToArray());

            return mn;
        }

        public static Mnozestvo Obied(params Mnozestvo[] mns)
        {
            Mnozestvo mn = new Mnozestvo('Z');

            mn.Elements = GetDistict(mns.Select(t => t.Elements).ToArray());

            return mn;
        }

        public static Mnozestvo Razn(params Mnozestvo[] mns) // х принадлежит 1 и не принадлежит остальным
        {
            Mnozestvo mn = new Mnozestvo('Z');  

            mn.Elements = GetRazn(mns.Select(t => t.Elements).ToArray());

            return mn;
        }

        public static Mnozestvo SRazn(params Mnozestvo[] mns)
        {
            Mnozestvo mn = new Mnozestvo('Z');

            mn.Elements = GetSRazn(mns.Select(t => t.Elements).ToArray());

            return mn;
        }

        public static Mnozestvo Dopoln(params Mnozestvo[] mns)
        {
            Mnozestvo mn = new Mnozestvo('Z');

            mn.Elements = GetDopoln(mns.Select(t => t.Elements).ToArray());

            return mn;
        }

        public static bool Ravno(params Mnozestvo[] mns)
        { 
            return false;
        }

        public static int[] GetSeq(int[] a1, int[] a2)
        {
            return a1.Intersect(a2).ToArray();
        }

        public static int[] GetSeq(params int[][] arrs)
        {
            var Odins = GetDistict(arrs); //1 2 3 1 2 5 2 4 5
            List<int> Odins1 = new List<int>(); //1 2 3  1 2 5  2 4 5

            foreach (var item in Odins)
            {
                if (IsSet(item, arrs))
                    Odins1.Add(item);
            }

            return Odins1.ToArray();
        }

        public static int[] GetDistict(params int[][] arrs)
        {
            List<int> Odins = new List<int>(); //1 2 3 1 2 5 2 4 5

            foreach (var i1 in arrs)
                for (int i = 0; i < i1.Length; i++)
                    Odins.Add(i1[i]);

            return Odins.Distinct().ToArray();
        }

        public static int[] GetRazn(params int[][] arrs)
        {
            List<int> Odins = new List<int>();  //1 2 3  1 2 5  2 4 5

            foreach (var i1 in arrs[0])
                if (IsNotSet(i1, arrs.Skip(1).ToArray())) 
                    Odins.Add(i1);

            return Odins.ToArray();
        }

        public static int[] GetRazn(int[] a1, int[] a2)
        {
            return a1.Except(a2).ToArray();
        }

        public static int[] GetSRazn(params int[][] arrs) //3 8 4 5
        {
            var obied = GetDistict(arrs);
            var peres = GetSeq(arrs);
            var razn = GetRazn(obied, peres);
            return razn;
        }

        public static int[] GetDopoln(params int[][] arrs) //3 8 4 5
        {
            var razn = GetRazn(Mnozestvo.U.Elements, GetDistict(arrs));
            return razn;
        }

        public static bool IsSet(int elem, params int[] arrs)
        {
            return arrs.Contains(elem);
        }

        public static bool IsSet(int elem, params int[][] arrs)
        {
            bool isf = true;
            foreach (var item in arrs)
                if (!IsSet(elem, item)) isf = false;

            return isf;
        }
        public static bool IsNotSet(int elem, params int[][] arrs)
        {
            bool isf = true;
            foreach (var item in arrs)
                if (IsSet(elem, item)) isf = false;

            return isf;
        }
    }
    class Program
    {
        const string text1 = "Введите выражение: ";
        static string text2 = "Условные обозначения: \n" + Mnozestvo.Peres + " - Пересечение\n"
            + Mnozestvo.Obed + " - Объединение\n"
            + Mnozestvo.Semetr + " - Семетричная разность\n"
            + Mnozestvo.Razn + " - Разность\n"
            + Mnozestvo.Pust + " - Пустое множество\n"
            + Mnozestvo.Dopoln + " - Дополнение\n";
        static void Main(string[] args)
        {
            //var t = Mnozestvo.Parse("A={1,3,7,8} ∩ {1,4,5,7} ∩ {7}");
            //var t3 = TextZapis.Parsing("A={1,3,7,8} ∩ {1,4,5,7} ∪ {7,9}");
            //var t4 = TextZapis.Execute(t3);
            //var t2 = Mnozestvo.U;

            Console.WriteLine(text2);
            while (true)
            {
                Console.WriteLine(text1);
                Console.SetCursorPosition(text1.Length, Console.CursorTop-1);
                string vir = Console.ReadLine().ToUpper().Trim();
                var t3 = TextZapis.Parsing(vir);
                var t4 = TextZapis.Execute(t3);
                Console.WriteLine(t4);
                Console.WriteLine("");

            }


            //Console.ReadLine();
        }
    }

    public class TZ
    {
        public enum TypeTZ
        {
            Operators,
            Mnozestvo,
        }

        public TypeTZ Types;
    }
}
