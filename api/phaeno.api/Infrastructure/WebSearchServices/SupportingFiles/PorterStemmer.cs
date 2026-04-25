namespace phaeno.api.Infrastructure.WebSearchServices.SupportingFiles
{
    public class PorterStemmer
    {
        private char[] b;
        private int i,     /* offset into b */
                    i_end, /* offset to end of stemmed word */
                    j, k;
        private static int INC = 50;

        public PorterStemmer()
        {
            b = new char[INC];
            i = 0;
            i_end = 0;
        }

        public void Add(char ch)
        {
            if (i == b.Length)
            {
                Array.Resize(ref b, i + INC);
            }
            b[i++] = ch;
        }

        public void Add(char[] w, int wLen)
        {
            if (i + wLen >= b.Length)
            {
                Array.Resize(ref b, i + wLen + INC);
            }
            Array.Copy(w, 0, b, i, wLen);
            i += wLen;
        }

        public override string ToString()
        {
            return new string(b, 0, i_end);
        }

        public int GetResultLength()
        {
            return i_end;
        }

        public char[] GetResultBuffer()
        {
            return b;
        }

        private bool Cons(int i)
        {
            switch (b[i])
            {
                case 'a': case 'e': case 'i': case 'o': case 'u': return false;
                case 'y': return i == 0 ? true : !Cons(i - 1);
                default: return true;
            }
        }

        private int M()
        {
            int n = 0;
            int i = 0;
            while (true)
            {
                if (i > j) return n;
                if (!Cons(i)) break; i++;
            }
            i++;
            while (true)
            {
                while (true)
                {
                    if (i > j) return n;
                    if (Cons(i)) break;
                    i++;
                }
                i++;
                n++;
                while (true)
                {
                    if (i > j) return n;
                    if (!Cons(i)) break;
                    i++;
                }
                i++;
            }
        }

        private bool VowelInStem()
        {
            for (int i = 0; i <= j; i++)
            {
                if (!Cons(i)) return true;
            }
            return false;
        }

        private bool DoubleC(int j)
        {
            if (j < 1) return false;
            if (b[j] != b[j - 1]) return false;
            return Cons(j);
        }

        private bool Cvc(int i)
        {
            if (i < 2 || !Cons(i) || Cons(i - 1) || !Cons(i - 2)) return false;
            int ch = b[i];
            if (ch == 'w' || ch == 'x' || ch == 'y') return false;
            return true;
        }

        private bool Ends(string s)
        {
            int l = s.Length;
            int o = k - l + 1;
            if (o < 0) return false;
            for (int i = 0; i < l; i++)
                if (b[o + i] != s[i]) return false;
            j = k - l;
            return true;
        }

        private void SetTo(string s)
        {
            int l = s.Length;
            int o = j + 1;
            for (int i = 0; i < l; i++) b[o + i] = s[i];
            k = j + l;
        }

        private void R(string s)
        {
            if (M() > 0) SetTo(s);
        }

        private void Step1()
        {
            if (b[k] == 's')
            {
                if (Ends("sses")) k -= 2;
                else if (Ends("ies")) SetTo("i");
                else if (b[k - 1] != 's') k--;
            }
            if (Ends("eed")) { if (M() > 0) k--; }
            else if ((Ends("ed") || Ends("ing")) && VowelInStem())
            {
                k = j;
                if (Ends("at")) SetTo("ate");
                else if (Ends("bl")) SetTo("ble");
                else if (Ends("iz")) SetTo("ize");
                else if (DoubleC(k))
                {
                    k--;
                    int ch = b[k];
                    if (ch == 'l' || ch == 's' || ch == 'z') k++;
                }
                else if (M() == 1 && Cvc(k)) SetTo("e");
            }
        }

        private void Step2()
        {
            if (Ends("y") && VowelInStem()) b[k] = 'i';
        }

        private void Step3()
        {
            if (k == 0) return;
            switch (b[k - 1])
            {
                case 'a':
                    if (Ends("ational")) { R("ate"); break; }
                    if (Ends("tional")) { R("tion"); break; }
                    break;
                case 'c':
                    if (Ends("enci")) { R("ence"); break; }
                    if (Ends("anci")) { R("ance"); break; }
                    break;
                case 'e':
                    if (Ends("izer")) { R("ize"); break; }
                    break;
                case 'l':
                    if (Ends("bli")) { R("ble"); break; }
                    if (Ends("alli")) { R("al"); break; }
                    if (Ends("entli")) { R("ent"); break; }
                    if (Ends("eli")) { R("e"); break; }
                    if (Ends("ousli")) { R("ous"); break; }
                    break;
                case 'o':
                    if (Ends("ization")) { R("ize"); break; }
                    if (Ends("ation")) { R("ate"); break; }
                    if (Ends("ator")) { R("ate"); break; }
                    break;
                case 's':
                    if (Ends("alism")) { R("al"); break; }
                    if (Ends("iveness")) { R("ive"); break; }
                    if (Ends("fulness")) { R("ful"); break; }
                    if (Ends("ousness")) { R("ous"); break; }
                    break;
                case 't':
                    if (Ends("aliti")) { R("al"); break; }
                    if (Ends("iviti")) { R("ive"); break; }
                    if (Ends("biliti")) { R("ble"); break; }
                    break;
                case 'g':
                    if (Ends("logi")) { R("log"); break; }
                    break;
            }
        }

        private void Step4()
        {
            switch (b[k])
            {
                case 'e':
                    if (Ends("icate")) { R("ic"); break; }
                    if (Ends("ative")) { R(""); break; }
                    if (Ends("alize")) { R("al"); break; }
                    break;
                case 'i':
                    if (Ends("iciti")) { R("ic"); break; }
                    break;
                case 'l':
                    if (Ends("ical")) { R("ic"); break; }
                    if (Ends("ful")) { R(""); break; }
                    break;
                case 's':
                    if (Ends("ness")) { R(""); break; }
                    break;
            }
        }

        private void Step5()
        {
            if (k == 0) return;
            switch (b[k - 1])
            {
                case 'a': if (Ends("al")) break; return;
                case 'c': if (Ends("ance") || Ends("ence")) break; return;
                case 'e': if (Ends("er")) break; return;
                case 'i': if (Ends("ic")) break; return;
                case 'l': if (Ends("able") || Ends("ible")) break; return;
                case 'n': if (Ends("ant") || Ends("ement") || Ends("ment") || Ends("ent")) break; return;
                case 'o':
                    if (Ends("ion") && j >= 0 && (b[j] == 's' || b[j] == 't') || Ends("ou")) break; return;
                case 's': if (Ends("ism")) break; return;
                case 't': if (Ends("ate") || Ends("iti")) break; return;
                case 'u': if (Ends("ous")) break; return;
                case 'v': if (Ends("ive")) break; return;
                case 'z': if (Ends("ize")) break; return;
                default: return;
            }
            if (M() > 1) k = j;
        }

        private void Step6()
        {
            j = k;
            if (b[k] == 'e')
            {
                int a = M();
                if (a > 1 || a == 1 && !Cvc(k - 1)) k--;
            }
            if (b[k] == 'l' && DoubleC(k) && M() > 1) k--;
        }

        public void Stem()
        {
            k = i - 1;
            if (k > 1)
            {
                Step1(); Step2(); Step3(); Step4(); Step5(); Step6();
            }
            i_end = k + 1;
            i = 0;
        }

        public string Stem(string word)
        {
            var stemmer = new PorterStemmer();

            foreach (char ch in word)
                stemmer.Add(ch);

            stemmer.Stem();

            return stemmer.ToString();
        }
    }
}
