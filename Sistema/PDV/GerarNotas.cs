﻿using Conn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace PDV
{
    public partial class GerarNotas: Form
    {
        public GerarNotas()
        {
            InitializeComponent();
        }
        Conn.Class1 conex = new Class1();
        private Dictionary<int, List<string>> xml = new Dictionary<int, List<string>>();
        private int IRetorno;
        int iRetorno = 0;
        int ACK, ST1, ST2, ST3;
        string chaveAcesso, numeroCupom, NumeroSAT, message, code, errorMessage, errorCode;
        string Cpf, DataEmit = null;
        private void GerarNotas_Load(object sender, EventArgs e)
        {
           
            int i = 0;
            

            //FileStream fs = new FileStream("CFe35170525168664000195590002954060002714556005.xml", FileMode.Open, FileAccess.Read);
            DirectoryInfo Dir = new DirectoryInfo(Application.StartupPath + @"\2017\");
            FileInfo[] Files = Dir.GetFiles("*.xml", SearchOption.AllDirectories);
            foreach (FileInfo File in Files)
            {
                FileStream fs = new FileStream(File.FullName, FileMode.Open, FileAccess.Read);
                try
                {
                    
                     XmlDataDocument xmldoc = new XmlDataDocument();
                     XmlNodeList xmlnode;
                    xmldoc.Load(fs);
                    xmlnode = xmldoc.GetElementsByTagName("CPF");
                    Cpf = xmlnode[0].ChildNodes.Item(0).InnerText.Trim();
                    xmlnode = xmldoc.GetElementsByTagName("dEmi");
                    DataEmit = xmlnode[0].ChildNodes.Item(0).InnerText.ToString().Trim();
                    xml.Add(i, new List<string> { Cpf, DataEmit });
                    i++;
                    
                }
                catch (Exception ex)
                {

                }
                
                
            }
            VerificaExisteCupom();
        }
        
        public bool VerificaExisteCupom()
        {
            bool aberto = false;
            string sQuery = null;
            sQuery += " select a.HANDLE,a.CPF,a.IMPRESSO,a.DATA_CADASTRO,b.VALOR ";
            sQuery += " from p_fluxo_caixa a  ";
            sQuery += " join p_pagamento_fluxo_caixa b on b.FLUXO_CAIXA =a.HANDLE ";
            sQuery += " where a.CPF!='' ";
            //sQuery += " where a.CPF='" + cpf + "' ";
            //sQuery += " and  (CONVERT(varchar, a.DATA_CADASTRO, 103) = CONVERT(date, '" + pdata + "', 103) ";
            sQuery += " order by a.DATA_CADASTRO desc ";
            OleDbConnection DbConnection = conex.Cnncontrol();
            OleDbCommand cmd = new OleDbCommand(sQuery, DbConnection);
            OleDbDataReader da = cmd.ExecuteReader();
            while (da.Read())
            {
                var pesquisaxml = xml.Where(s => s.Value.Contains(da["CPF"].ToString().Trim()) && s.Value.Contains(string.Format("{0:yyyymmdd}", da["DATA_CADASTRO"])));
                if (pesquisaxml == null)
                {
                    bool deucerto = true;
                    //int ACK, ST1, ST2, ST3;
                    #region ABRECUPOM

                    double TotalImposto = 0;

                    iRetorno = BemaFI32.Bematech_FI_AbreCupomMFD(da["CPF"].ToString(), "", "");
                    
                    iRetorno = BemaFI32.Bematech_FI_RetornoImpressoraMFD(ref ACK, ref ST1, ref ST2, ref ST3);
                    if (ACK == 0)
                    {
                        MessageBox.Show("Erro de comunicação com a impressora!");
                        deucerto = false;
                    }

                    if (ST3 == 66)
                    {
                        MessageBox.Show("Redução Z pendente!");
                        if (MessageBox.Show("Deseja Emitir a redução Z agora?", "PDV", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            iRetorno = BemaFI32.Bematech_FI_ReducaoZ("", "");
                        }
                        deucerto = false;
                    }

                    if (ST3 == 7)
                    {
                        deucerto = true;
                    }


                    #endregion

                    #region ITEM

                    if (BemaFI32.Bematech_FI_RetornoImpressoraMFD(ref ACK, ref ST1, ref ST2, ref ST3) > 0)
                    {
                        sQuery += " select ";
                        sQuery += " e.HANDLE CODPRODUTO ";
                        sQuery += " ,e.NOME PRODUTO ";
                        sQuery += " ,d.VALOR ";
                        sQuery += " from  p_produto_fluxo_caixa d  ";
                        sQuery += " join p_produtos e on d.PRODUTO = e.HANDLE ";
                        sQuery += " where d.FLUXO_CAIXA= " + da["HANDLE"];
                        cmd = new OleDbCommand(sQuery, DbConnection);
                        OleDbDataReader drp = cmd.ExecuteReader();
                        while (drp.Read())
                        {
                            string Codigo = drp["CODPRODUTO"].ToString();
                            string EAN13 = "";
                            string Descricao = drp["PRODUTO"].ToString();
                            string IndiceDepartamento = "00";
                            string Aliquota = "F1";
                            string UnidadeMedida = "UN";
                            string TipoQuantidade = "I";// (Pdv.dataGridView1[8, linha.Index].Value.ToString() == "2 - Inteiro" ? "I" : "F");
                            string CasasDecimaisQtde = "3";
                            string Quantidade = "1";
                            string CasasDecimaisValor = "2";
                            string ValorUnitario = drp["VALOR"].ToString();
                            string TipoDesconto = "$";
                            string ValorAcrescimo = "0,00";
                            string ValorDesconto = "0,00";
                            string ArredondaTrunca = "A";
                            string NCM = "22011000";
                            string CFOP = "5403";
                            string InformacaoAdicional = "OBRIGADO E VOLTE SEMPRE!!!";
                            string OrigemProduto = "0";
                            string CST_ICMS = "60";
                            string CodigoIBGE = "";
                            string CodigoISS = "";
                            string NaturezaOperacaoISS = "";
                            string IndicadorIncentivoFiscal = "";
                            string ItemListaServico = "1234";
                            string CSOSN = "102"; //--> Simples Nacional
                            string ValorBaseCalculoSimples = "0";
                            string ValorICMSRetidoSimples = "0";
                            string ModalidadeBaseCalculo = "0";
                            string PercentualReducaoBase = "0";
                            string ModalidadeBC = "0";
                            string PercentualMargemICMS = "0";
                            string PercentualBCICMS = "0";
                            string ValorReducaoBCICMS = "0";
                            string ValorAliquotaICMS = "0";
                            string ValorICMS = "0";
                            string ValorICMSDesonerado = "0";
                            string MotivoDesoneracaoICMS = "0";
                            string AliquotaCalculoCredito = "0";
                            string ValorCreditoICMS = "0";
                            string ValorTotalTributos = ""; //tributos
                            string CSTPIS = "04";
                            string BaseCalculoPIS = "";
                            string AliquotaPIS = "";
                            string ValorPIS = "";
                            string QuantVendidaPIS = "";
                            string ValorAliquotaPIS = "";
                            string CSTCOFINS = "04";
                            string BaseCalculoCOFINS = "";
                            string AliquotaCOFINS = "";
                            string ValorCOFINS = "";
                            string QunatVendidaCOFINS = "";
                            string ValorAliquotaCOFINS = "";
                            string Reservado01 = "";
                            string Reservado02 = "";
                            string Reservado03 = "";
                            string Reservado04 = "";
                            string Reservado05 = "";
                            string Reservado06 = "";
                            string Reservado07 = "";
                            string Reservado08 = "";
                            string Reservado09 = "";
                            string Reservado10 = "";


                            iRetorno = BemaFI32.Bematech_FI_VendeItemCompleto(Codigo, EAN13, Descricao, IndiceDepartamento, Aliquota, UnidadeMedida, TipoQuantidade, CasasDecimaisQtde, Quantidade, CasasDecimaisValor, ValorUnitario, TipoDesconto, ValorAcrescimo, ValorDesconto, ArredondaTrunca, NCM, CFOP, InformacaoAdicional, CST_ICMS, OrigemProduto, ItemListaServico, CodigoISS, NaturezaOperacaoISS, IndicadorIncentivoFiscal, CodigoIBGE, CSOSN, ValorBaseCalculoSimples, ValorICMSRetidoSimples, ModalidadeBaseCalculo, PercentualReducaoBase, ModalidadeBC, PercentualMargemICMS, PercentualBCICMS, ValorReducaoBCICMS, ValorAliquotaICMS, ValorICMS, ValorICMSDesonerado, MotivoDesoneracaoICMS, AliquotaCalculoCredito, ValorCreditoICMS, ValorTotalTributos, CSTPIS, BaseCalculoPIS, AliquotaPIS, ValorPIS, QuantVendidaPIS, ValorAliquotaPIS, CSTCOFINS, BaseCalculoCOFINS, AliquotaCOFINS, ValorCOFINS, QunatVendidaCOFINS, ValorAliquotaCOFINS, Reservado01, Reservado02, Reservado03, Reservado04, Reservado05, Reservado06, Reservado07, Reservado08, Reservado09, Reservado10);
                            //BemaFI32.Analisa_iRetorno(iRetorno);
                            //MessageBox.Show("VendeItemCompleto: " + iRetorno);
                            iRetorno = BemaFI32.Bematech_FI_RetornoImpressoraMFD(ref ACK, ref ST1, ref ST2, ref ST3);
                            //MessageBox.Show("ACK: " + ACK + " - " + "ST1: " + ST1 + " - " + "ST2: " + ST2 + " - " + "ST3: " + ST3);
                        }
                        drp.Close();
                        
                    }



                    #endregion

                    #region INICIO FECHAMENTO
                    iRetorno = BemaFI32.Bematech_FI_IniciaFechamentoCupom("A", "%", "00");
                    BemaFI32.Analisa_iRetorno(iRetorno);
                    //MessageBox.Show("IniciaFechamentoCupom: " + iRetorno);
                    iRetorno = BemaFI32.Bematech_FI_RetornoImpressoraMFD(ref ACK, ref ST1, ref ST2, ref ST3);
                    //MessageBox.Show("ACK: " + ACK + " - " + "ST1: " + ST1 + " - " + "ST2: " + ST2 + " - " + "ST3: " + ST3);
                    #endregion

                    #region FORMA PAGAMENTO
                        sQuery = " select ";
                        sQuery += " FORMAPAGAMENTO ";
                        sQuery += " ,VALOR ";
                        sQuery += " from p_pagamento_fluxo_caixa  ";
                        sQuery += " where d.FLUXO_CAIXA= " + da["HANDLE"];
                        cmd = new OleDbCommand(sQuery, DbConnection);
                        OleDbDataReader drf = cmd.ExecuteReader();
                        while (drf.Read())
                        {
                            IRetorno = BemaFI32ImpressoraFiscal.Bematech_FI_EfetuaFormaPagamento(drf["FORMAPAGAMENTO"].ToString(), drf["VALOR"].ToString());
                            BemaFI32.Analisa_iRetorno(iRetorno);
                            //MessageBox.Show("EfetuaFormaPagamento: " + iRetorno);
                            iRetorno = BemaFI32.Bematech_FI_RetornoImpressoraMFD(ref ACK, ref ST1, ref ST2, ref ST3);
                            //MessageBox.Show("ACK: " + ACK + " - " + "ST1: " + ST1 + " - " + "ST2: " + ST2 + " - " + "ST3: " + ST3);
                        }
                        drf.Close();
                    #endregion

                    #region DADOS Software HOUSE
                    string cnpj = "19823348000107";
                    string assinaturaSW = "QEd6c058zUvRZ/To5BRZcLj6a1bwu0bx1pRhYyR50k2DxYnxcrnt9dfazWDjyaWQYS8/YB1xQFnmxXOJOkY+/YbKXn0g+SODZ1ptlzL0s1vTQxn/qKZCPcSC+vaBp7J09kNsegffT5JIF93QcXfo+IoM52zALIGz+rwE/BA5vZUVA4YnF3bl8NztG+08W7Ziz1s+GHfc+UGBxAIefAsb8bNPdJFj32fcwLCmhvTdAg0VpKi9F6rdiOkcYVo9I2Ej0tZIAiGxUO84dWm8M28hF+Rmr+QlwRPnKbWGNCGT3Gy1pQzYk28wM/IuaDPEcMrkVrQWeXHEZA22zhMYvA+6ww==";
                    iRetorno = BemaFI32.Bematech_FI_DadosSoftwareHouseSAT(cnpj, assinaturaSW);
                    //MessageBox.Show("DADOS DA SH"+ iRetorno);
                    iRetorno = BemaFI32.Bematech_FI_RetornoImpressoraMFD(ref ACK, ref ST1, ref ST2, ref ST3);
                    #endregion

                    #region FECHAMENTO
                    iRetorno = BemaFI32.Bematech_FI_TerminaFechamentoCupom("OBRIGADO E VOLTE SEMPRE!");
                    BemaFI32.Analisa_iRetorno(iRetorno);
                    //MessageBox.Show("TerminaFechamentoCupom: " + iRetorno);
                    iRetorno = BemaFI32.Bematech_FI_RetornoImpressoraMFD(ref ACK, ref ST1, ref ST2, ref ST3);
                    #endregion
                }
            }
            
            
            da.Close();
            
            return aberto;
        }


    }
}