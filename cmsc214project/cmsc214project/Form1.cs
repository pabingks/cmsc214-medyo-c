﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections;

namespace cmsc214project
{
    public partial class Form1 : Form
    {
        string[] tokens;                //accumulated tokens read from the code textbox
        string cToken;                  //current token
        int cIndex;                     //current index
        int cLine;                      //current line in the tokens array
        Boolean error = false;          //error flag
        Boolean fFlag = false;          //float flag
        Boolean closed = false;

        Form2 f2 = new Form2();

        Stack<String> stack = new Stack<String>();
        Stack<String> expr = new Stack<String>();
		//symbol table for storing values of variables
		Hashtable symbolTable = new Hashtable(); 
		
        /*
         * accumulators for arithmetic computations 
         * (similar to ax in assembly)
        */
        Double acc = 0.0;//store arithmetic values here
        Double acx = 1.0;
        Boolean abc = false;
        string ans = "";
        Boolean onAcc = false;//boolean value to determine if accumulator is being used

        public Form1()
        {
            InitializeComponent();
        }

        /*
         * Clears the code textbox
         */
        private void clear_Click(object sender, EventArgs e)
        {
            code.Text = "";
            resetValues();
        }

        private void run_Click(object sender, EventArgs e)
        {
            int i,j;

            //Resets values of all variables
            resetValues();

            tokens = code.Text.TrimEnd().Split('\n');

            //Trims whitespaces in every token
            for (i = 0; i < tokens.Length; i++)
            {
                tokens[i] = Regex.Replace(tokens[i], @"^[0-9]*", "");
                tokens[i] = tokens[i].Trim(' ', '\r', '\n', '\t');
                tokens[i] = tokens[i].Replace('\t', ' ');       
            }

            String[] test_in = {"4","5","*","6","*","IPAKITA",
                                "4","5","-","IPAKITANA","VAR","IKUHA"};
            String[] a = {"1","2","-","3","+"};
            lexer();
            parse();
            
            if (error == false)
            {
                cLine = 0;
                cIndex = 0;
                clearAccumuators();
                expr.Clear();
                lexer();
                output.AppendText("Parsed Successfully!\n");
                eval();
            }

           //Display line numbers
            code.Text = "";
            for (i = 0; i < tokens.Length; i++)
            {
                j = i + 1;
                if (i == tokens.Length - 1) code.AppendText(j + "   " + tokens[i]);
                else code.AppendText(j + "   " + tokens[i] + "\r\n");
            }
        }

        /*
         * Gets the current token per character
        */
        private void lexer()
        {
            string temp = "";
            cToken = "";
            //checks if end of line for the code
            if (cLine < tokens.Length)
            {
                //checks if the current index is still within the bounds of the current line in the code
                if (cIndex < tokens[cLine].Length)
                {
                    
                    cToken = tokens[cLine][cIndex].ToString();
                    skipSpaces();
                    
                    //if there's nothing in that line
                    if (cToken.Length == 0 && tokens[cLine].Length == 0)
                    {
                        cLine++;
                        cIndex = 0;
                        lexer();
                    }
                    else
                    {
                        if(cToken == "<")
                        {
                            cIndex++;
                            if (cIndex < tokens[cLine].Length)
                            {
                                cToken = tokens[cLine][cIndex].ToString();
                                cIndex++;
                                if (cIndex < tokens[cLine].Length && cToken == "3")
                                {
                                    cToken = tokens[cLine][cIndex].ToString();
                                    cIndex++;
                                    while (cIndex < tokens[cLine].Length && cToken != "<")
                                    {
                                        if (cIndex < tokens[cLine].Length) cToken = tokens[cLine][cIndex].ToString();
                                        cIndex++;
                                    }
                                    if (cIndex < tokens[cLine].Length && cToken == "<")
                                    {
                                        cToken = tokens[cLine][cIndex].ToString();
                                        cIndex++;
                                        if (cToken == "3")
                                        {
                                            lexer();
                                        }
                                        else
                                        {
                                            displayError("Inaasahang <3");
                                        }
                                    }
                                }
                                else
                                {
                                    cIndex--;
                                    cToken = tokens[cLine][cIndex - 1].ToString();
                                }
                            }
                            else
                            {
                                displayError("Imbalidong salita");
                            }
                        }//end of comment
                        //checks for string
                        else if (cToken == "\"")
                        {
                            
                            cIndex++;
                            if (cIndex < tokens[cLine].Length)
                            {
                                cToken = tokens[cLine][cIndex].ToString();
                                while (cToken != "\"" && cIndex < tokens[cLine].Length)
                                {
                                    temp = temp + cToken;
                                    cIndex++;

                                    if (cIndex < tokens[cLine].Length)
                                    {
                                        cToken = tokens[cLine][cIndex].ToString();
                                    }
                                }

                                //if successfully caught a string
                                if (cToken == "\"")
                                {
                                    cToken = "\"" + temp + "\"";
                                    // if (!parseFlag) tokenize();
                                    cIndex++;
                                }
                                else
                                {
                                    if (error == false)
                                    {
                                        displayError("Kulang o Inaasahang \"");
                                    }
                                }
                            }
                            else
                            {
                                displayError("Kulang o Inaasahang \"");
                            }
                        }//end of string
                        //check for character
                        else if (cToken == "\'")
                        {
                            cIndex++;
                            
                            if (cIndex < tokens[cLine].Length)
                            {
                                cToken = tokens[cLine][cIndex].ToString();
                                temp = cToken;
                                cIndex++;
                                if (cIndex < tokens[cLine].Length)
                                {
                                    cToken = tokens[cLine][cIndex].ToString();
                                    if (cToken == "\'")
                                    {
                                        cToken = "\'" + temp + "\'";
                                        cIndex++;
                                    }
                                    else
                                    {
                                        displayError("Kulang o Inaasahang \'");
                                    }
                                }
                                else
                                {
                                    displayError("Kulang o Inaasahang \'");
                                }
                            }
                            else
                            {
                                displayError("Kulang o Inaasahang \'");
                            }
                        }
                        else if (cLine < tokens.Length && cIndex < tokens[cLine].Length)
                        {
                            while (cToken != " " && cIndex < tokens[cLine].Length)
                            {
                                temp = temp + cToken;
                                cIndex++;

                                if (cIndex < tokens[cLine].Length)
                                {
                                    cToken = tokens[cLine][cIndex].ToString();
                                }
                            }
                            cToken = temp;
                        }//end of cToken
                    }
                } //end of if (cIndex < tokens[cLine].Length)
                else
                {
                    //if end of line, move to the next line

                    cIndex = 0;
                    cLine++;
                    lexer();
                }
            }
        }

        private void parse()
        {
            
            if(cLine < tokens.Length && error == false)
            {
                
                if (checkVar() || checkScan() || checkPrint() || checkAssign() || checkIf() || checkLoop())
                {
                    //code.AppendText("+" + cToken + "+");
                    //output.AppendText("Parsed! :)\n");
                }
                else
                {
                    displayError("Imbalidong salita");
                }
                if (cLine < tokens.Length && error == false) parse();
            }
        }

        private void eval()
        {
            if (cLine < tokens.Length && error == false)
            {
                if (cToken == "IPAKITA")
                {
                    lexer();
                    checkExpr();
                    /*output.AppendText(expr.Count().ToString());
                    while (expr.Count()!=0)
                    {
                        output.AppendText(expr.Pop());
                    }*/
                    evalNumExpr();
                    output.AppendText(ans);
                }
                else if (cToken == "IPAKITANA")
                {
                    lexer();
                    checkExpr();
                    evalNumExpr();
                    output.AppendText(ans + Environment.NewLine);
                }
                else if (cToken == "IKUHA")
                {
                    lexer();
                    if (variableExists(cToken))
                    {
                        //get variable type of cToken
                        Tuple<string, string> t = getVariableContent(cToken);
                        String varType = t.Item1;

                        f2.setVarType(varType);
                       
                        //get value from user
                        String value = readInput();

                        changeVarValue(cToken, value);
                    }
                }
                else if (cToken == "GAWIN")
                {
                    lexer();
					checkExpr();
					evalNumExpr();
					eval(); 
                }

                lexer();
               if (cLine < tokens.Length && error == false) eval();
            }
            
        }

        private void evalNumExpr()
        {
            //initialize stack
            Stack<Object> s = new Stack<Object>();
            Double result = 0;
            String ex = "";

            if (expr.Count() < 2 && expr.Count() > 0)
            {
                string temp = expr.Pop();
                if (Double.TryParse(temp, out result))
                {
                    acc = result;
                    ans = acc.ToString();
                }
                else if(temp == "totoo")
                {
                    abc = true;
                    ans = "totoo";
                }
                else if (temp == "peke")
                {
                    abc = false;
                    ans = "peke";
                }
               return;
            }

            while (expr.Count() != 0)
            {
                /**
                    fetch part
                **/
                String i = expr.Pop();
                while (true)
                {
                    //check if input is a number
                    if (Double.TryParse(i, out result))
                    {
                        s.Push(result);
                    }

                    //check if input is a string literal
                    /*else if(){
                    }*/

                    //check if input is a variable
                    else if (variableExists(i))
                    {
                        s.Push(i);
                    }

                    //input is a command
                    else
                    {
                        ex = i;
                        break;
                    }

                    //get next lexeme
                    i = expr.Pop();
                }

                /**
                 * execute part
                 * Check what kind of instruction
                 **/

                /** Arithmetic Operations**/
                //addition operator
                if (ex == "+")
                {
                    s.Push((Double)s.Pop() + (Double)s.Pop());
                }

                //subtraction operator
                else if (ex == "-")
                {
                    s.Push((Double)s.Pop() - (Double)s.Pop());
                }

                //multiplication operator
                else if (ex == "*")
                {
                    s.Push((Double)s.Pop() * (Double)s.Pop());
                }

                //division operator
                else if (ex == "/")
                {
                    s.Push((Double)s.Pop() / (Double)s.Pop());
                }

                //modulo operator
                else if (ex == "%")
                {
                    s.Push((Double)s.Pop() % (Double)s.Pop());
                }

                /** Logical Operations**/
                else if (ex == "<")
                {
                    abc = (Double)s.Pop() < (Double)s.Pop() ? true : false;
                    if (abc == true) ans = "totoo";
                    else ans = "peke";
                } 
                else if (ex == ">") 
                {
                    abc = (Double)s.Pop() > (Double)s.Pop() ? true : false;
                    if (abc == true) ans = "totoo";
                    else ans = "peke";
                } 
                else if (ex == "<=")
                {
                    abc = (Double)s.Pop() <= (Double)s.Pop() ? true : false;
                    if (abc == true) ans = "totoo";
                    else ans = "peke";
                } 
                else if (ex == ">=")
                {
                    abc = (Double)s.Pop() >= (Double)s.Pop() ? true : false;
                    if (abc == true) ans = "totoo";
                    else ans = "peke";
                }
                else if (ex == "=")
                {
                    abc = (Double)s.Pop() == (Double)s.Pop() ? true : false;
                    if (abc == true) ans = "totoo";
                    else ans = "peke";
                }
            }
            if (s.Count() != 0)
            {
                acc = (Double)s.Pop();
                ans = acc.ToString();
            }
        }

         /*Get Type and Content of a certain variable*/
        private Tuple<string,string> getVariableContent(String varname){
            Tuple<string,string> t = (Tuple<string,string>)symbolTable[varname];
            return t;
        }

        /*Check if variable already exists*/
        private Boolean variableExists(String varname){
            if(symbolTable.ContainsKey(varname))
                return true;//variable exists
            else return false;
        }

        /*Change value of a variable*/
        private void changeVarValue(String varname, String value)
        {
            Tuple<string, string> t = (Tuple<string, string>)symbolTable[varname];
            Tuple<string, string> newValue = new Tuple<string, string>(t.Item1,value);

            //replace with new value
            symbolTable[varname] = newValue;
            //printSymbolTable();
        }

        /* Print values of the hash table*/
        private void printSymbolTable(){
            foreach(DictionaryEntry entry in symbolTable){
                String varname = (String)entry.Key;
                Tuple<string,string> t = getVariableContent(varname);

                String vartype = t.Item1;
                String value = t.Item2;

                output.AppendText(Environment.NewLine+"Variable Name: "+varname+" ");
                output.AppendText("Type: "+vartype+" Value: "+t.Item2+Environment.NewLine);
            }
        }

        /*
         * Function for storing variables in the symbol table
         */
        private void storeVar(String cType, String cVar, String cValue)
        {
            //tuple for storing variable type and value
            Tuple<string,string> value = new Tuple<string,string>(cType,cValue);
            
            //store variable name, type and value in the symbol table
            symbolTable.Add(cVar,value);

            //printSymbolTable();
        }

        /**Read input from user**/
        private String readInput()
        {
            f2.ShowDialog();
            String input = f2.textBox1.Text;

            closed = f2.isClosed();
            f2.textBox1.Text = "";//reset text
			
			return input;
        }

        /*
         * Checks if statement is variable declaration
         */
        private Boolean checkVar()
        {
            string[] dataTypes = { "INTEDYER", "PLOWT", "BULYAN", "ISTRING", "KAR" };
            int line;
            string cType = "";
            string cVar = "";

            if (isDataType(cToken))
            {
                cType =  cToken;
                line = cLine;
                lexer();
                if (line==cLine && isVarName() && !variableExists(cToken))
                {
                    cVar = cToken;
                    lexer();

                    if (line == cLine && cToken == "AY")
                    {
                        lexer();
                        
                        if (cType == "INTEDYER" && checkExpr('A') && fFlag == false)
                        {
                            evalNumExpr();
                            storeVar(cType, cVar, acc.ToString());
                            lexer();
                            return true;
                        }
                        else if (cType == "PLOWT" && checkExpr('A') && fFlag == true)
                        {
                            evalNumExpr();
                            storeVar(cType, cVar, acc.ToString());
                            lexer();
                            return true;
                        }
                        else if (cType == "BULYAN" && checkExpr('L'))
                        {
                            evalNumExpr();
                            if(abc == true) storeVar(cType, cVar, "totoo");
                            else if (abc == false) storeVar(cType, cVar, "peke");
                            lexer();
                            return true;
                        }
                        else if(cType == "ISTRING" && cToken[0] == '\"' && cToken[cToken.Length-1] == '\"')
                        {
                            storeVar(cType, cVar, cToken.Substring(1,cToken.Length-2));
                            lexer();
                            return true;
                        }
                        else if (cType == "KAR" && cToken[0] == '\'' && cToken[cToken.Length - 1] == '\'')
                        {
                            storeVar(cType, cVar, cToken[1].ToString());
                            lexer();
                            return true;
                        }
                        else
                        {
                            displayError("Magkasalungat na uri");
                        }
                    }
                    else
                    {
                        if (line != cLine)
                        {
                            storeVar(cType, cVar, "BETEL");
                            return true;
                        }
                        else if (line == cLine) displayError("Nawawala o Inaasahang AY");
                    }
                }
                else
                {
                    if (line != cLine)
                    {
                        displayError("Nawawala o Inaasahang pangalan ng baryante");
                    }
                    else if (!isVarName())
                    {
                        displayError("Imbalidong pangalan ng baryante");
                    }
                    else if (variableExists(cToken))
                    {
                        displayError("'" + cToken + "' ay naideklarang baryante ulit");
                    }
                    
                }
            }

            return false;
        }

        /*
         * Checks if statement is a print statement
         */
        private Boolean checkPrint()
        {
            if (cToken == "IPAKITA" || cToken=="IPAKITANA")
            {
                lexer();
                if (checkExpr())
                {
                    lexer();
                    return true;
                }
            }
            return false;
        }

        /*
         * Checks if statement is a scan statement
         */
        private Boolean checkScan()
        {
            if (cToken == "IKUHA")
            {
                lexer();
                if (variableExists(cToken))
                {
                    lexer();
                    return true;
                }
                else
                {
                    displayError("'"+cToken+"' ay hindi naideklarang baryante");
                }
            }
            return false;
        }

        /*
         * Checks if statement is an assignment statement
         */
        private Boolean checkAssign()
        {
            string cVar = "";
            string cType = "";
            if (variableExists(cToken))
            {
                cVar = cToken;
                cType = getVariableContent(cVar).Item1;
                lexer();
                if (cToken == "AY")
                {
                    lexer();

                    if (cType == "INTEDYER" && checkExpr('A') && fFlag == false)
                    {
                        evalNumExpr();
                        changeVarValue(cVar, acc.ToString());
                        lexer();
                        return true;
                    }
                    else if (cType == "PLOWT" && checkExpr('A') && fFlag == true)
                    {
                        evalNumExpr();
                        changeVarValue(cVar, acc.ToString());
                        lexer();
                        return true;
                    }
                    else if (cType == "BULYAN" && checkExpr('L'))
                    {
                        evalNumExpr();
                        if (abc == true) changeVarValue(cVar, "totoo");
                        else if (abc == false) changeVarValue(cVar, "peke");
                        lexer();
                        return true;
                    }
                    else if (cType == "ISTRING" && cToken[0] == '\"' && cToken[cToken.Length - 1] == '\"')
                    {
                        changeVarValue(cVar, cToken.Substring(1, cToken.Length - 2));
                        lexer();
                        return true;
                    }
                    else if (cType == "KAR" && cToken[0] == '\'' && cToken[cToken.Length - 1] == '\'')
                    {
                        changeVarValue(cVar, cToken[1].ToString());
                        lexer();
                        return true;
                    }
                    else
                    {
                        displayError("Magkasalungat na uri");
                    }
                }
                else
                {
                    displayError("Nawawala o Inaasahang AY");
                }
            }
            return false;
        }

        /*
         * Checks if statement is an if-else statement
         */
        private Boolean checkIf()
        {
            
            if (cToken == "KUNG")
            {
                lexer();
                if (checkExpr())
                {
                    lexer();
                    if (cToken == "[")
                    {
                        lexer();
                        while (cToken != "]" && cLine < tokens.Length && error == false)
                        {
                            if (!(checkVar() || checkScan() || checkIf() || checkAssign() || checkPrint() || checkLoop()))
                            {
                                displayError("Imbalidong salita");
                                break;
                            }
                        }

                        if (cToken == "]")
                        {

                            lexer();
                            if (cToken == "EDI")
                            {
                                lexer();
                                if (cToken == "KUNG")
                                {
                                    checkIf();
                                }
                                else
                                {
                                    if (cToken == "[")
                                    {
                                        lexer();
                                        while (cToken != "]" && cLine < tokens.Length && error == false)
                                        {
                                            if (!(checkVar() || checkScan() || checkIf() || checkAssign() || checkPrint() || checkLoop()))
                                            {
                                                displayError("Imbalidong salita");
                                                break;
                                            }
                                        }
                                        if (cToken == "]")
                                        {
                                            lexer();
                                            return true;
                                        }
                                        else
                                        {
                                            displayError("Nawawala o Inaaasahang ]");
                                        }
                                    }
                                    else
                                    {
                                        displayError("Nawawala o Inaaasahang [");
                                    }
                                }
                            }
                            if (error == false) return true;
                        }
                        else
                        {
                            displayError("Nawawala o Inaaasahang ]");
                        }
                    }
                    else
                    {
                        displayError("Nawawala o Inaaasahang [");
                    }
                }
                else
                {
                    displayError("Nawawala o Inaasahang ekspresyon");
                }
            }
            return false;
        }

        /*
         * Checks if statement is an iterative statement
         */
        private Boolean checkLoop()
        {
            if (cToken == "GAWIN")
            {
                lexer();
                if (!((variableExists(cToken) && getVariableContent(cToken).Item1 == "INTEDYER") || checkExpr('A')))
                {
                    displayError("Inaasahan ang INTEDYER ngunit ang ekspresyon ay hindi ang inaasahan");
                    return false;
                }
                else
                {
                    lexer();

                    if (cToken == "[")
                    {
                        lexer();
                        while (cToken != "]" && cLine < tokens.Length && error == false)
                        {
                            if (!(checkVar() || checkScan() || checkIf() || checkAssign() || checkPrint() || checkLoop()))
                            {
                                displayError("Imbalidong salita");
                                break;
                            }
                        }

                        if (cToken == "]")
                        {
                            lexer();
                            return true;
                        }
                        else
                        {
                            displayError("Nawawala o Inaaasahang ]");
                        }
                    }
                    else
                    {
                        displayError("Nawawala o Inaaasahang [");
                    }
                }
                /*else
                {
                    displayError("Inaasahan ang INTEDYER ngunit ang ekspresyon ay hindi ang inaasahan");
                    return false;
                }*/
            }
            return false;
        }

        private Boolean checkExpr(Char expType = 'L')
        {
            if (error) return false;
            Regex exp = new Regex(@"^[-]?(\d*[.])?\d+$");
            Match m = exp.Match(cToken);
            if (m.Success)
            {
                if (cToken.Contains("."))
                {
                    fFlag = true;
                }
                expr.Push(cToken);
                return true;
            }
            else if(cToken == "totoo" || cToken == "peke")
            {
                expr.Push(cToken);
                return true;
            } 
            else if (cToken[0] == '\"' && cToken[cToken.Length-1] == '\"')
            {
                ans = cToken.Substring(1, cToken.Length - 2);
                return true;
            }
            else if (cToken[0] == '\'' && cToken[cToken.Length - 1] == '\'')
            {
                ans = cToken[1].ToString();
                return true;
            }
            else if ((cToken == "*" || cToken == "-" || cToken == "+" || cToken == "/" || cToken == "%"))
            {
                expr.Push(cToken);
                lexer(); checkExpr('A');
                lexer(); checkExpr('A');
            }
            else if (expType == 'L' && (cToken == ">" || cToken == ">=" || cToken == "<=" || cToken == "=" || cToken == "<"))
            {
                expr.Push(cToken);
                lexer(); checkExpr('L');
                lexer(); checkExpr('L');
            }
            else if (expType == 'A' && variableExists(cToken))
            {
                if (getVariableContent(cToken).Item1 == "PLOWT") fFlag = true;
                if (getVariableContent(cToken).Item1 != "INTEDYER" && getVariableContent(cToken).Item1 != "PLOWT")
                {
                    displayError("Inaasahan ang INTEDYER o PLOWT ngunit ang baryante ay hindi ang inaasahan");
                    expr.Clear();
                    return false;
                }
                else
                {
                    expr.Push(getVariableContent(cToken).Item2);
                    return true;
                }
            }
            else if (expType == 'L' && variableExists(cToken))
            {
                if (getVariableContent(cToken).Item1 == "PLOWT") fFlag = true;
                if (getVariableContent(cToken).Item1 != "INTEDYER" && getVariableContent(cToken).Item1 != "PLOWT" && getVariableContent(cToken).Item1 != "BULYAN" && getVariableContent(cToken).Item1 != "KAR" && getVariableContent(cToken).Item1 != "ISTRING")
                {
                    displayError("Inaasahan ang INTEDYER o PLOWT o BULYAN ngunit ang baryante ay hindi ang inaasahan");
                    expr.Clear();
                    return false;
                }
                else
                {
                    if (getVariableContent(cToken).Item1 == "INTEDYER" || getVariableContent(cToken).Item1 == "PLOWT" || getVariableContent(cToken).Item1 == "BULYAN") expr.Push(getVariableContent(cToken).Item2);
                    return true;
                }
                
            }
            else if (variableExists(cToken))
            {
                if (getVariableContent(cToken).Item1 != "BULYAN" && getVariableContent(cToken).Item1 != "KAR" && getVariableContent(cToken).Item1 != "ISTRING")
                {
                    displayError("Inaasahan ang ibang bagay ngunit ang iniligay ay \'" + cToken + "\'");
                    expr.Clear();
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                displayError("Inaasahan ang ibang bagay ngunit ang iniligay ay \'" + cToken + "\'");
                expr.Clear();
                return false;
            }
            return true;
        }

        /*
         * Resets all the global variables
         */
        private void resetValues()
        {
            clearAccumuators();
            ans = "";
            cToken = "";
            cLine = 0;
            cIndex = 0;
            error = false;
            fFlag = false;
            output.Text = "";
            expr.Clear();
            stack.Clear();
            output.ForeColor = Color.White;
            symbolTable.Clear();//reset hashtable
        }

        /*
         * Moves the cToken to a non-whitespace character in a line
         */
        private void skipSpaces()
        {
            while (cIndex < tokens[cLine].Length)
            {
                if (cToken == " " || cToken == "\t")
                {
                    cIndex++;
                    cToken = tokens[cLine][cIndex].ToString();
                }
                else
                {
                    break;
                }
            }
        }

        /*
         * Displays an error message to the output textbox
         */
        private void displayError(String errorMessage)
        {
            int i;

            if (error == false)
            {
                i = cLine + 1;
                output.ForeColor = Color.Red;
                output.Text = "MALI SA LINYA " + i + ": " + errorMessage;
                error = true;

                //end the interpreter
                cLine = tokens.Length - 1;
                cIndex = tokens[cLine].Length;
            }
        }

        /*
         * Checks the validity of the data type
         */
        private Boolean isDataType(string cToken)
        {
            int i;
            string[] dataTypes = { "INTEDYER", "PLOWT", "BULYAN", "ISTRING", "KAR" };
            //cToken = cToken.ToUpper();

            for(i=0; i<dataTypes.Length; i++)
            {
                if(cToken == dataTypes[i]) return true;
            }
            
            return false;
        }

        /*
         * Checks the validity of the variable name
         */
        private Boolean isVarName()
        {
            Regex exp = new Regex(@"^[A-Za-z]+.*$");
            Match m = exp.Match(cToken);

            if (m.Success)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void clearAccumuators()
        {
            abc = false;
            acc = 0.0;
            acx = 1.0;
            onAcc = false;
        }

        private void evaluate_code(String[] cmd)
        {
            //initialize stack
            Stack<Object> s = new Stack<Object>();
            Double result = 0;

            String ex = "";
            int ctr = 0;

            while (ctr < cmd.Length)
            {
                /**
                    fetch part
                **/
                String i = cmd[ctr];

                /**
                    analyze part
                    habang hindi pa command... (variable or literal)
                    get operands and operators
                **/
                while (true)
                {
                    //check if input is a number
                    if (Double.TryParse(i, out result))
                    {
                        s.Push(result);
                    }

                    //check if input is a string literal
                    /*else if(){
                    }*/

                    //check if input is a variable
                    else if(variableExists(i))
                    {
						s.Push(i);
                    }

                    //input is a command
                    else
                    {
                        ex = i;
                        break;
                    }

                    //get next lexeme
                    i = cmd[++ctr];
                }

                /**
                 * execute part
                 * Check what kind of instruction
                 **/

                /** Arithmetic Operations**/
                //addition operator
                if (ex == "+")
                {
                    onAcc = true;//turn on accumulator
                    while (s.Count != 0)
                    {
                        acc += (Double)s.Pop();
                    }
                }

                //subtraction operator
                else if (ex == "-")
                {
                    while (s.Count != 0)
                    {
                        if (onAcc)
                            acc -= (Double)s.Pop();
                        //it is the very first operand
                        else
                        {
                            acc = (Double)s.Pop();
                            onAcc = true;//turn on accumulator
                        }
                    }
                }

                //multiplication operator
                else if (ex == "*")
                {
                    while (s.Count != 0)
                    {
                        acx *= (Double)s.Pop();
                    }

                    //determine kung paano isasama sa accumulator...
                    if (onAcc)
                    {
                        acc *= acx;
                    }
                    //it is the very first operand
                    else
                    {
                        onAcc = true;//turn on accumulator
                        acc = acx;
                    }

                    acx = 1;
                }

                //division operator
                else if (ex == "/")
                {
                    while (s.Count != 0)
                    {
                        acx /= (Double)s.Pop();
                    }

                    //determine kung paano isasama sa accumulator...
                    if (onAcc)
                    {
                        acc *= acx;
                    }
                    //it is the very first operand
                    else
                    {
                        onAcc = true;//turn on accumulator
                        acc = acx;
                    }

                    acx = 1;
                }

                //modulo operator
                else if (ex == "%")
                {
                    while (s.Count != 0)
                    {
                        acx %= (Double)s.Pop();
                    }

                    //determine kung paano isasama sa accumulator...
                    if (onAcc)
                    {
                        acc *= acx;
                    }
                    //it is the very first operand
                    else
                    {
                        onAcc = true;//turn on accumulator
                        acc = acx;
                    }

                    acx = 1;
                }

                /** I/O Functions**/
                //one-line printing function
                else if (ex == "IPAKITA")
                {
                    //for printing arithmetic values
                    output.Text += acc;

                    //clear artihmetic accumulators
                    clearAccumuators();
                }

                //printing with newline function (System.out.println...)
                else if (ex == "IPAKITANA")
                {
                    //for printing arithmetic values with newline
                    output.Text += (Environment.NewLine + acc);

                    //clear arithmetic accumulators
                    clearAccumuators();
                }

                //getting input
                else if(ex == "IKUHA"){
					//get variable name from stack
					String varname = (String)s.Pop();
					
					//get type of the variable...
					Tuple<string,string> t = getVariableContent(varname);
					String vartype = t.Item1;
					
					//pass vartype to Form2
					f2.setVarType(vartype);
					
					//get value from user
					String value = readInput();

                    output.Text += Environment.NewLine+"Variable name: "+varname+" Value: "+value;

					//assign input value to the variable
					changeVarValue(varname,value);
                }

                ctr++;//move to the next lexeme
            }
        }
    }
}
