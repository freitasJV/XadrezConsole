﻿using System;
using System.Collections.Generic;
using System.Linq;
using XadrezConsole.Board;

namespace XadrezConsole.Xadrez
{
    class PartidaXadrez
    {
        public Tabuleiro Tab { get; private set; }
        public int Turno { get; private set; }
        public Cor JogadorAtual { get; private set; }
        public bool Finalizada { get; private set; }
        public bool IsXeque { get; private set; }
        public Peca VulneravelEnPassant { get; private set; }

        private HashSet<Peca> Pecas;
        private HashSet<Peca> Capturadas;
        

        public PartidaXadrez()
        {
            Tab = new Tabuleiro(8, 8);
            Turno = 1;
            JogadorAtual = Cor.Branca;
            Finalizada = false;
            IsXeque = false;
            VulneravelEnPassant = null;
            Pecas = new HashSet<Peca>();
            Capturadas = new HashSet<Peca>();
            ColocarPecas();
        }

        public void RealizaJogada(Posicao origem, Posicao destino)
        {
            Peca capturada = ExecutaMovimento(origem, destino);

            if(isXeque(JogadorAtual))
            {
                DesfazMovimento(origem, destino, capturada);
                throw new TabuleiroException("Você não pode se colocar em xeque!");
            }

            Peca p = Tab.Peca(destino);

            // #JogadaEspecial Promocao

            if(p is Peao)
            {
                if(p.Cor == Cor.Branca && destino.Linha == 0 || p.Cor == Cor.Preta && destino.Linha == 7)
                {
                    p = Tab.RetirarPeca(destino);
                    Pecas.Remove(p);
                    Peca dama = new Dama(Tab, p.Cor);
                    Tab.ColocarPeca(dama, destino);
                }
            }


            if (isXeque(Adversaria(JogadorAtual)))
                IsXeque = true;
            else
                IsXeque = false;

            if (IsXequeMate(Adversaria(JogadorAtual)))
                Finalizada = true;
            else
            {
                Turno++;
                MudarJogador();
            }

            // #jogadaEspecial en passant
            if (p is Peao && (destino.Linha == origem.Linha - 2 || destino.Linha == origem.Linha + 2))
                VulneravelEnPassant = p;
            else
                VulneravelEnPassant = null;
        }

        private void DesfazMovimento(Posicao origem, Posicao destino, Peca capturada)
        {
            Peca p = Tab.RetirarPeca(destino);
            p.SubtractMovimento();

            if (capturada != null)
            {
                Tab.ColocarPeca(capturada, destino);
                Capturadas.Remove(capturada);
            }
            Tab.ColocarPeca(p, origem);

            // #jogada especial roque pequeno

            if (p is Rei && destino.Coluna == origem.Coluna + 2)
            {
                Posicao origemT = new Posicao(origem.Linha, origem.Coluna + 3);
                Posicao destinoT = new Posicao(origem.Linha, origem.Coluna + 1);
                Peca T = Tab.RetirarPeca(destinoT);
                T.SubtractMovimento();
                Tab.ColocarPeca(T, origemT);
            }

            // #jogada especial roque grande

            if (p is Rei && destino.Coluna == origem.Coluna - 2)
            {
                Posicao origemT = new Posicao(origem.Linha, origem.Coluna - 4);
                Posicao destinoT = new Posicao(origem.Linha, origem.Coluna - 1);
                Peca T = Tab.RetirarPeca(destinoT);
                T.SubtractMovimento();
                Tab.ColocarPeca(T, origemT);
            }

            //#jogadaEspecial en passant
            if (p is Peao)
            {
                if (origem.Coluna != destino.Coluna && capturada == VulneravelEnPassant)
                {
                    Peca peao = Tab.RetirarPeca(destino);
                    Posicao posP;
                    if (p.Cor == Cor.Branca)
                        posP = new Posicao(3, destino.Coluna);
                    else
                        posP = new Posicao(4, destino.Coluna);

                    Tab.ColocarPeca(peao, posP);
                }
            }
        }

        public void ValidarPosicaoDeOrigem(Posicao pos)
        {
            if (Tab.Peca(pos) == null)
                throw new TabuleiroException("Não existe peça na posição de origem escolhida!");

            if(JogadorAtual != Tab.Peca(pos).Cor)
                throw new TabuleiroException("A peça de origem escolhida não é sua!");

            if(!Tab.Peca(pos).ExisteMovimentosPossiveis())
                throw new TabuleiroException("Não há movimentos possíveis para a peça de origem escolhida!");
        }

        public void ValidarPosicaoDeDestino(Posicao origem, Posicao destino)
        {
            if(!Tab.Peca(origem).MovimentoPossivel(destino))
                throw new TabuleiroException("Posição de destino inválida!");
        }

        private void MudarJogador()
        {
            if (JogadorAtual == Cor.Branca)
                JogadorAtual = Cor.Preta;
            else
                JogadorAtual = Cor.Branca;
        }

        public Peca ExecutaMovimento(Posicao origem, Posicao destino)
        {
            Peca p = Tab.RetirarPeca(origem);
            p.AddMovimento();
            Peca capturada = Tab.RetirarPeca(destino);
            Tab.ColocarPeca(p, destino);

            if (capturada != null)
                Capturadas.Add(capturada);

            // #jogada especial roque pequeno

            if(p is Rei && destino.Coluna == origem.Coluna + 2)
            {
                Posicao origemT = new Posicao(origem.Linha, origem.Coluna + 3);
                Posicao destinoT= new Posicao(origem.Linha, origem.Coluna + 1);
                Peca T = Tab.RetirarPeca(origemT);
                T.AddMovimento();
                Tab.ColocarPeca(T, destinoT);
            }

            // #jogada especial roque grande

            if (p is Rei && destino.Coluna == origem.Coluna - 2)
            {
                Posicao origemT = new Posicao(origem.Linha, origem.Coluna - 4);
                Posicao destinoT = new Posicao(origem.Linha, origem.Coluna - 1);
                Peca T = Tab.RetirarPeca(origemT);
                T.AddMovimento();
                Tab.ColocarPeca(T, destinoT);
            }

            //#jogadaEspecial en passant
            if(p is Peao)
            {
                if (origem.Coluna != destino.Coluna && capturada == null)
                {
                    Posicao posP;
                    if (p.Cor == Cor.Branca)
                        posP = new Posicao(destino.Linha + 1, destino.Coluna);
                    else
                        posP = new Posicao(destino.Linha - 1, destino.Coluna);

                    capturada = Tab.RetirarPeca(posP);
                    Capturadas.Add(capturada);
                }
            }

            return capturada;
        }

        public HashSet<Peca> PecasCapturadas(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();

            foreach(Peca x in Capturadas)
            {
                if (x.Cor == cor)
                    aux.Add(x);
            }

            return aux;
        }

        public HashSet<Peca> PecasEmJogo(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();

            foreach (Peca x in Pecas)
            {
                if (x.Cor == cor)
                    aux.Add(x);
            }

            aux.ExceptWith(PecasCapturadas(cor));

            return aux;
        }

        private Cor Adversaria(Cor cor)
        {
            if (cor == Cor.Branca)
                return Cor.Preta;
            else
                return Cor.Branca;
        }

        private Peca rei(Cor cor)
        {
            foreach(Peca x in PecasEmJogo(cor))
            {
                if (x is Rei)
                    return x;
            }

            return null;
        }

        public bool isXeque(Cor cor)
        {
            Peca R = rei(cor);

            foreach(Peca x in PecasEmJogo(Adversaria(cor)))
            {
                bool[,] mat = x.MovimentosPossiveis();
                if (mat[R.Posicao.Linha, R.Posicao.Coluna])
                    return true;
            }

            return false;
        }

        public bool IsXequeMate(Cor cor)
        {
            if (!isXeque(cor))
                return false;

            foreach(Peca x in PecasEmJogo(cor))
            {
                bool[,] mat = x.MovimentosPossiveis();
                for (int i = 0; i < Tab.Linhas; i++)
                {
                    for (int j = 0; j < Tab.Colunas; j++)
                    {
                        if (mat[i, j])
                        {
                            Posicao origem = x.Posicao;
                            Posicao destino = new Posicao(i, j);
                            Peca pecaCapturada = ExecutaMovimento(origem, destino);
                            bool testeXeque = isXeque(cor);
                            DesfazMovimento(origem, destino, pecaCapturada);

                            if (!testeXeque)
                                return false;
                        }
                    }
                }
            }
            return true;
        }
    
        public void ColocarNovaPeca(char coluna, int linha, Peca peca)
        {
            Tab.ColocarPeca(peca, new PosicaoXadrez(coluna, linha).toPosicao());
            Pecas.Add(peca);
        }

        private void ColocarPecas()
        {
            ColocarNovaPeca('a', 1, new Torre(Tab, Cor.Branca));
            ColocarNovaPeca('b', 1, new Cavalo(Tab, Cor.Branca));
            ColocarNovaPeca('c', 1, new Bispo(Tab, Cor.Branca));
            ColocarNovaPeca('d', 1, new Dama(Tab, Cor.Branca));
            ColocarNovaPeca('e', 1, new Rei(Tab, Cor.Branca,this));
            ColocarNovaPeca('f', 1, new Bispo(Tab, Cor.Branca));
            ColocarNovaPeca('g', 1, new Cavalo(Tab, Cor.Branca));
            ColocarNovaPeca('h', 1, new Torre(Tab, Cor.Branca));
            ColocarNovaPeca('a', 2, new Peao(Tab, Cor.Branca,this));
            ColocarNovaPeca('b', 2, new Peao(Tab, Cor.Branca,this));
            ColocarNovaPeca('c', 2, new Peao(Tab, Cor.Branca,this));
            ColocarNovaPeca('d', 2, new Peao(Tab, Cor.Branca,this));
            ColocarNovaPeca('e', 2, new Peao(Tab, Cor.Branca,this));
            ColocarNovaPeca('f', 2, new Peao(Tab, Cor.Branca,this));
            ColocarNovaPeca('g', 2, new Peao(Tab, Cor.Branca,this));
            ColocarNovaPeca('h', 2, new Peao(Tab, Cor.Branca, this));

            ColocarNovaPeca('a', 8, new Torre(Tab, Cor.Preta));
            ColocarNovaPeca('b', 8, new Cavalo(Tab, Cor.Preta));
            ColocarNovaPeca('c', 8, new Bispo(Tab, Cor.Preta));
            ColocarNovaPeca('d', 8, new Dama(Tab, Cor.Preta));
            ColocarNovaPeca('e', 8, new Rei(Tab, Cor.Preta, this));
            ColocarNovaPeca('f', 8, new Bispo(Tab, Cor.Preta));
            ColocarNovaPeca('g', 8, new Cavalo(Tab, Cor.Preta));
            ColocarNovaPeca('h', 8, new Torre(Tab, Cor.Preta));
            ColocarNovaPeca('a', 7, new Peao(Tab, Cor.Preta,this));
            ColocarNovaPeca('b', 7, new Peao(Tab, Cor.Preta,this));
            ColocarNovaPeca('c', 7, new Peao(Tab, Cor.Preta,this));
            ColocarNovaPeca('d', 7, new Peao(Tab, Cor.Preta,this));
            ColocarNovaPeca('e', 7, new Peao(Tab, Cor.Preta,this));
            ColocarNovaPeca('f', 7, new Peao(Tab, Cor.Preta,this));
            ColocarNovaPeca('g', 7, new Peao(Tab, Cor.Preta,this));
            ColocarNovaPeca('h', 7, new Peao(Tab, Cor.Preta, this));
        }
    }
}
