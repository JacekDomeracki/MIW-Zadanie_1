using System;
using System.Collections.Generic;
using System.Linq;

namespace Zadanie_1
{
    public class Osobnik
    {
        public byte[] param_1;
        public byte[] param_2;
        public double wart_fun_przyst = 0;

        public Osobnik(int ile_chrom)
        {
            param_1 = new byte[ile_chrom];
            param_2 = new byte[ile_chrom];
        }

        private void Losuj_parametr(byte[] param)
        {
            Random random = new Random();
            for (int i = 0; i < param.Length; i++)
            {
                param[i] = (byte)random.Next(2);
            }
        }

        public void Losuj_wszystkie_parametry()
        {
            Losuj_parametr(param_1);
            Losuj_parametr(param_2);
        }

        private int Konwersja_bin2dec_param(byte[] param)
        {
            int param_dec = 0;
            for (int i = 0; i < param.Length; i++)
            {
                param_dec += param[i] * (int)Math.Pow(2, i);            //LSB ... MSB
            }
            return param_dec;
        }

        private double Funkcja_przystosowania(double x1, double x2)
        {
            return Math.Round(Math.Sin(x1 * 0.05) + Math.Sin(x2 * 0.05) + 0.4 * Math.Sin(x1 * 0.15) * Math.Sin(x2 * 0.15), 6);
        }

        public void Oblicz_funkcje_przystosowania(Dictionary<int, double> przedzial_dyskretny)
        {
            wart_fun_przyst = Funkcja_przystosowania(przedzial_dyskretny[Konwersja_bin2dec_param(param_1)], przedzial_dyskretny[Konwersja_bin2dec_param(param_2)]);
        }

        public void SkopiujZ(Osobnik osob_do_skopiowania, int ile_chrom)
        {
            for (int i = 0; i < ile_chrom; i++)
            {
                this.param_1[i] = osob_do_skopiowania.param_1[i];
                this.param_2[i] = osob_do_skopiowania.param_2[i];
            }
            this.wart_fun_przyst = osob_do_skopiowania.wart_fun_przyst;
        }

        private void Pokaz_parametr(byte[] param)
        {
            for (int i = param.Length - 1; i >= 0; i--)
            {
                Console.Write("{0,3}", param[i]);               //MSB ... LSB
            }
        }

        public void Pokaz_wszystkie_parametry(Dictionary<int, double> przedzial_dyskretny)
        {
            Pokaz_parametr(param_1);
            Console.Write("  |");
            Pokaz_parametr(param_2);
            Console.Write("  |");
            Console.Write("{0,5}", Konwersja_bin2dec_param(param_1));
            Console.Write("  |");
            Console.Write("{0,5}", Konwersja_bin2dec_param(param_2));
            Console.Write("  |");
            Console.Write("{0,8:F2}", przedzial_dyskretny[Konwersja_bin2dec_param(param_1)]);
            Console.Write("  |");
            Console.Write("{0,8:F2}", przedzial_dyskretny[Konwersja_bin2dec_param(param_2)]);
            Console.Write("  |");
            Console.Write("{0,12:F6}", wart_fun_przyst);
            Console.WriteLine("  |");
        }
    }

    internal class Program
    {
        const double PRZEDZ_MIN = 0;
        const double PRZEDZ_MAX = 100;
        //const int ILE_PARAM = 2;
        const int ILE_CHROM_NP = 5;
        const int ILE_OSOB = 17;
        const int ILE_OS_TUR = 3;
        const int ILE_ITERACJE = 20;

        static void Dyskretyzacja_przedzialu(Dictionary<int, double> przedzial_dyskretny, double przedz_min, double przedz_max, int ile_chrom)
        {
            int dyskretny_max = (int)Math.Pow(2, ile_chrom) - 1;
            double delta = Math.Round((przedz_max - przedz_min) / dyskretny_max, 4);        //dokładność zaokrąglenia ma znaczenie
            przedzial_dyskretny.Add(0, przedz_min);
            przedzial_dyskretny.Add(dyskretny_max, przedz_max);
            for (int i = 1; i < dyskretny_max; i++)
            {
                przedzial_dyskretny.Add(i, przedz_min + i * delta);
            }
        }

        static void Operator_selekcji_hot_deck(Osobnik[] pula_osobnikow, ref Osobnik osob_hot_deck, int ile_chrom)
        {
            Osobnik osob_hot_deck_rob;
            osob_hot_deck_rob = pula_osobnikow[0];
            for (int i = 1; i < pula_osobnikow.Length; i++)
            {
                if (osob_hot_deck_rob.wart_fun_przyst < pula_osobnikow[i].wart_fun_przyst) osob_hot_deck_rob = pula_osobnikow[i];
            }
            osob_hot_deck.SkopiujZ(osob_hot_deck_rob, ile_chrom);
        }

        static void Operator_selekcji_turniejowej(Osobnik[] pula_osobnikow, ref Osobnik osob_zwyc_turnieju, int ile_osob_tur, int ile_chrom)
        {
            List<int> indeksy_puli = new List<int>();
            for (int i = 0; i < pula_osobnikow.Length; i++)
            {
                indeksy_puli.Add(i);
            }
            Random random = new Random();
            int i_ip = random.Next(indeksy_puli.Count);
            int n_osob = indeksy_puli[i_ip];
            indeksy_puli.Remove(n_osob);
            int n_osob_rywal;
            for (int i = 0; i < ile_osob_tur - 1; i++)
            {
                i_ip = random.Next(indeksy_puli.Count);
                n_osob_rywal = indeksy_puli[i_ip];
                indeksy_puli.Remove(n_osob_rywal);
                if (pula_osobnikow[n_osob].wart_fun_przyst < pula_osobnikow[n_osob_rywal].wart_fun_przyst) n_osob = n_osob_rywal;
            }
            osob_zwyc_turnieju.SkopiujZ(pula_osobnikow[n_osob], ile_chrom);
        }

        static void Operator_mutowanie(ref Osobnik osob_zmutowany, int ile_chrom, Dictionary<int, double> przedzial_dyskretny)
        {
            Random random = new Random();
            int n_par = random.Next(1, 3);
            int n_bit = random.Next(ile_chrom);
            switch (n_par)
            {
                case 1:
                    osob_zmutowany.param_1[n_bit] = (byte)(1 - osob_zmutowany.param_1[n_bit]);
                    break;
                case 2:
                    osob_zmutowany.param_2[n_bit] = (byte)(1 - osob_zmutowany.param_2[n_bit]);
                    break;
            }
            osob_zmutowany.Oblicz_funkcje_przystosowania(przedzial_dyskretny);
        }

        static void Operator_krzyzowanie(ref Osobnik osob_krzyzow_1, ref Osobnik osob_krzyzow_2, int ile_chrom, Dictionary<int, double> przedzial_dyskretny)
        {
            byte rob;
            for (int i = 0; i < ile_chrom; i++)
            {
                rob = osob_krzyzow_1.param_2[i];
                osob_krzyzow_1.param_2[i] = osob_krzyzow_2.param_2[i];
                osob_krzyzow_2.param_2[i] = rob;
            }
            osob_krzyzow_1.Oblicz_funkcje_przystosowania(przedzial_dyskretny);
            osob_krzyzow_2.Oblicz_funkcje_przystosowania(przedzial_dyskretny);
        }

        static double Najlepsza_wart_fun_przyst(Osobnik[] pula_osobnikow)
        {
            double wart_fun_przyst = pula_osobnikow[0].wart_fun_przyst;
            for (int i = 1; i < pula_osobnikow.Length; i++)
            {
                if (wart_fun_przyst < pula_osobnikow[i].wart_fun_przyst) wart_fun_przyst = pula_osobnikow[i].wart_fun_przyst;
            }
            return wart_fun_przyst;
        }

        static double Srednia_wart_fun_przyst(Osobnik[] pula_osobnikow)
        {
            double sum_wart_fun_przyst = 0;
            for (int i = 0; i < pula_osobnikow.Length; i++)
            {
                sum_wart_fun_przyst += pula_osobnikow[i].wart_fun_przyst;
            }
            return Math.Round(sum_wart_fun_przyst / pula_osobnikow.Length, 6);
        }

        static void TEST_1(string nazwa, Dictionary<int, double> Przedzial_dyskretny)
        {
            Console.WriteLine("(TEST) " + nazwa);
            foreach (var p_dyskr in Przedzial_dyskretny.OrderBy(x => x.Key))
            {
                Console.WriteLine("{0,5}  -  {1,8:F6}", p_dyskr.Key, p_dyskr.Value);
            }
            Console.WriteLine();
        }

        static void TEST_2(string nazwa, Osobnik[] pula_osobnikow, Dictionary<int, double> Przedzial_dyskretny)
        {
            Console.WriteLine("(TEST) " + nazwa);
            for (int i = 0; i < pula_osobnikow.Length; i++)
            {
                Console.Write("{0,3}|", i + 1);
                pula_osobnikow[i].Pokaz_wszystkie_parametry(Przedzial_dyskretny);
            }
            Console.WriteLine();
        }

        static void TEST_3(string nazwa, Osobnik[] pula_osobnikow, ref Osobnik osobnik_rob_1, ref Osobnik osobnik_rob_2, Dictionary<int, double> Przedzial_dyskretny)
        {
            Console.WriteLine("(TEST) " + nazwa);

            Operator_selekcji_hot_deck(pula_osobnikow, ref osobnik_rob_1, ILE_CHROM_NP);
            Console.WriteLine("Hot Deck :");
            osobnik_rob_1.Pokaz_wszystkie_parametry(Przedzial_dyskretny);
            Console.WriteLine();

            Operator_selekcji_turniejowej(pula_osobnikow, ref osobnik_rob_1, ILE_OS_TUR, ILE_CHROM_NP);
            Console.WriteLine("Turniej :");
            osobnik_rob_1.Pokaz_wszystkie_parametry(Przedzial_dyskretny);
            Console.WriteLine();

            osobnik_rob_1.SkopiujZ(pula_osobnikow[ILE_OSOB - 3], ILE_CHROM_NP);
            Operator_mutowanie(ref osobnik_rob_1, ILE_CHROM_NP, Przedzial_dyskretny);
            Console.WriteLine("Zmutowanie :");
            osobnik_rob_1.Pokaz_wszystkie_parametry(Przedzial_dyskretny);
            Console.WriteLine("Przed mutacją :");
            pula_osobnikow[ILE_OSOB - 3].Pokaz_wszystkie_parametry(Przedzial_dyskretny);
            Console.WriteLine();

            osobnik_rob_1.SkopiujZ(pula_osobnikow[ILE_OSOB - 2], ILE_CHROM_NP);
            osobnik_rob_2.SkopiujZ(pula_osobnikow[ILE_OSOB - 1], ILE_CHROM_NP);
            Operator_krzyzowanie(ref osobnik_rob_1, ref osobnik_rob_2, ILE_CHROM_NP, Przedzial_dyskretny);
            Console.WriteLine("Skrzyżowane :");
            osobnik_rob_1.Pokaz_wszystkie_parametry(Przedzial_dyskretny);
            osobnik_rob_2.Pokaz_wszystkie_parametry(Przedzial_dyskretny);
            Console.WriteLine("Przed skrzyżowaniem :");
            pula_osobnikow[ILE_OSOB - 2].Pokaz_wszystkie_parametry(Przedzial_dyskretny);
            pula_osobnikow[ILE_OSOB - 1].Pokaz_wszystkie_parametry(Przedzial_dyskretny);
            Console.WriteLine();
        }

        static void Main()
        {
            Console.WriteLine("ALGORYTM GENETYCZNY ( ZADANIE 1 )");
            Console.WriteLine();

            Dictionary<int, double> Przedzial_dyskretny = new Dictionary<int, double>();
            Dyskretyzacja_przedzialu(Przedzial_dyskretny, PRZEDZ_MIN, PRZEDZ_MAX, ILE_CHROM_NP);
            TEST_1("Dyskretyzacja:", Przedzial_dyskretny);

            Osobnik[] pula_osobnikow = new Osobnik[ILE_OSOB];
            Osobnik[] nowa_pula_osobnikow = new Osobnik[ILE_OSOB];
            Osobnik[] pula_osobnikow_rob;
            Osobnik osobnik_rob_1 = new Osobnik(ILE_CHROM_NP);
            Osobnik osobnik_rob_2 = new Osobnik(ILE_CHROM_NP);

            for (int i = 0; i < ILE_OSOB; i++)
            {
                pula_osobnikow[i] = new Osobnik(ILE_CHROM_NP);
                pula_osobnikow[i].Losuj_wszystkie_parametry();
                pula_osobnikow[i].Oblicz_funkcje_przystosowania(Przedzial_dyskretny);

                nowa_pula_osobnikow[i] = new Osobnik(ILE_CHROM_NP);
            }
            TEST_2("Pula osobników:", pula_osobnikow, Przedzial_dyskretny);

            //TEST_3("Operatory genetyczne:", pula_osobnikow, ref osobnik_rob_1, ref osobnik_rob_2, Przedzial_dyskretny);

            Console.WriteLine("-->  START");
            Console.WriteLine("Najlepsza wartość funkcji przystosowania :{0,10:F6}", Najlepsza_wart_fun_przyst(pula_osobnikow));
            Console.WriteLine("  Średnia wartość funkcji przystosowania :{0,10:F6}", Srednia_wart_fun_przyst(pula_osobnikow));
            Console.WriteLine();

            for (int i = 0; i < ILE_ITERACJE; i++)
            {
                for (int j = 0; j < ILE_OSOB / 2; j++)          //z puli osobników bierzemy po 2 kolejne osobniki
                {
                    Operator_selekcji_turniejowej(pula_osobnikow, ref osobnik_rob_1, ILE_OS_TUR, ILE_CHROM_NP);         //zwycięzca pierwszego turnieju
                    Operator_selekcji_turniejowej(pula_osobnikow, ref osobnik_rob_2, ILE_OS_TUR, ILE_CHROM_NP);         //zwycięzca drugiego turnieju

                    Operator_mutowanie(ref osobnik_rob_1, ILE_CHROM_NP, Przedzial_dyskretny);           //zmutowanie pierwszego osobnika (przed skrzyżowaniem)
                    //Operator_mutowanie(ref osobnik_rob_2, ILE_CHROM_NP, Przedzial_dyskretny);

                    Operator_krzyzowanie(ref osobnik_rob_1, ref osobnik_rob_2, ILE_CHROM_NP, Przedzial_dyskretny);          //skrzyżowanie pierwszego i drugiego osobnika

                    nowa_pula_osobnikow[j * 2].SkopiujZ(osobnik_rob_1, ILE_CHROM_NP);
                    nowa_pula_osobnikow[j * 2 + 1].SkopiujZ(osobnik_rob_2, ILE_CHROM_NP);
                }
                if (ILE_OSOB % 2 == 1)          //tylko gdy nieparzysta liczba osobników w puli
                {
                    Operator_selekcji_hot_deck(pula_osobnikow, ref osobnik_rob_1, ILE_CHROM_NP);            //najlepszy osobnik w puli
                    nowa_pula_osobnikow[ILE_OSOB - 1].SkopiujZ(osobnik_rob_1, ILE_CHROM_NP);
                }
                Console.WriteLine("-->  ITERACJA NR :{0,3}", i + 1);
                Console.WriteLine("Najlepsza wartość funkcji przystosowania :{0,10:F6}", Najlepsza_wart_fun_przyst(nowa_pula_osobnikow));
                Console.WriteLine("  Średnia wartość funkcji przystosowania :{0,10:F6}", Srednia_wart_fun_przyst(nowa_pula_osobnikow));
                Console.WriteLine();

                pula_osobnikow_rob = pula_osobnikow;
                pula_osobnikow = nowa_pula_osobnikow;
                nowa_pula_osobnikow = pula_osobnikow_rob;
            }
            Console.WriteLine("-->  KONIEC");
            Console.WriteLine();
        }
    }
}
