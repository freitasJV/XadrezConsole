using XadrezConsole.Board;

namespace XadrezConsole
{
    class Tela
    {
        public static void ImprimirTabuleiro(Tabuleiro tab)
        {
            for (int i = 0; i < tab.Linhas; i++)
            {
                for (int j = 0; j < tab.Colunas; j++)
                {
                    if (tab.Peca(i, j) == null)
                    {
                        System.Console.Write("- ");
                    }
                    else
                    {
                        System.Console.Write($"{tab.Peca(i, j)} ");
                    }
                }
                System.Console.WriteLine();
            }
        }
    }
}
