using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Bumblebee_Compiler.Targets.RISC_Z {
    internal class RISC_Z_Compiler : ICompiler {
        public Dictionary<string, VariableOptions> KnownVariables => new(){
            {"input", new() {IsReadable = true, IsWritable = false, TypeName = "uint8"} },
            {"output", new() {IsReadable = false, IsWritable = true, TypeName = "uint8"} },
            {"HI", new() {IsReadable = true, IsWritable = true, TypeName = "uint8"} },
            {"counter", new() {IsReadable = true, IsWritable = true, TypeName = "uint8"} },
            {"stack", new() {IsReadable = true, IsWritable = true, TypeName = "uint8"} }
        };

        public Dictionary<string, FunctionOptions> KnownFunctions => new() {
            
        };

        StreamWriter writer = null;
        bool[] registersUsed = null;
        Dictionary<string, int> registers = null;
        TargetRegister targetRegister = new();
        uint nextLabel = 0;

        public void Compile(ASTNode program, StreamWriter outputFile) {
            writer = outputFile;
            registersUsed = new bool[20];
            registers = new();
            List<ASM> instructions = Compile(program).ToList();
            foreach(var instruction in instructions) {
                outputFile.WriteLine(instruction.ToString());
            }
        }

        struct TargetRegister {
            public int number = -1;
            public string name = null;
            public TargetRegister(int reg) {
                this.number = reg;
            }
            public TargetRegister(string name) {
                this.name = name;
            }
            public TargetRegister() { }
        }

        internal enum OpCode {
            add = 0b0,
            sub = 0b1,
            and = 0b10,
            or = 0b11,
            xor = 0b100,
            not = 0b101,
            lsh = 0b110,
            rsh = 0b111,
            mul = 0b1000,
            div = 0b1001,
            mod = 0b1010,
            load = 0b11111,
            jmp = 0b10000,
            jmp_eq = 0b10001,
            jmp_neq = 0b10010,
            jmp_gt = 0b10011,
            jmp_gte = 0b10100,
            jmp_lt = 0b10101,
            jmp_lte = 0b10110
        }

        struct ASM {
            /*public bool LoadArg0 = false;
            public bool LoadArg1 = false;
            public bool LoadArg2 = false;*/
            public OpCode Op;
            public string Arg0;
            public string Arg1;
            public string Arg2;
            public string Comment = null;
            public string Label = null;

            public ASM() { }

            public override string ToString() {
                if (Comment != null) return "# " + Comment;
                if (Label != null) return "label " + Label;

                string asm = Op.ToString();
                string arg0 = Arg0;
                string arg1 = Arg1;
                string arg2 = Arg2;
                int temp;
                if (int.TryParse(Arg0, out temp)) {
                    asm += "|arg0";
                    arg0 = Arg0;
                }
                if (int.TryParse(Arg1, out temp)) {
                    asm += "|arg1";
                    arg1 = Arg1;
                }
                if (int.TryParse(Arg2, out temp)) {
                    asm += "|arg2";
                    arg2 = Arg2;
                }

                switch (Op) {
                    case OpCode.add:
                    case OpCode.sub:
                    case OpCode.and:
                    case OpCode.or:
                    case OpCode.xor:
                    case OpCode.lsh:
                    case OpCode.rsh:
                    case OpCode.mul:
                    case OpCode.div:
                    case OpCode.mod:
                    case OpCode.jmp_eq:
                    case OpCode.jmp_neq:
                    case OpCode.jmp_gt:
                    case OpCode.jmp_gte:
                    case OpCode.jmp_lt:
                    case OpCode.jmp_lte:
                        asm += " " + arg0;
                        asm += " " + arg1;
                        asm += " " + arg2;
                        return asm;
                    case OpCode.not:
                    case OpCode.load:
                        asm += " " + arg0;
                        asm += " " + arg1;
                        return asm;
                    case OpCode.jmp:
                        asm += " " + arg0;
                        return asm;
                    default:
                        throw new Exception("Unknown opcode.");
                }
            }
        }

        private int GetUnusedRegister() {
            for (int i = 0; i < registersUsed.Length; i++) {
                if (registersUsed[i] == false) {
                    registersUsed[i] = true;
                    return i;
                }
            }
            throw new Exception("No remaining registers available.");
        }

        private string GetTargetRegisterName(TargetRegister register) {
            // TODO move into TargetRegister struct
            if (register.number >= 0) {
                return "reg" + register.number;
            } else if (register.name != null) {
                switch(register.name) {
                    case "input":
                    case "output":
                    case "HI":
                    case "counter":
                    case "stack":
                        return register.name;
                    default:
                        return "reg" + registers[register.name];
                }
            } else {
                throw new Exception("Register Null");
            }
        }

        private string GetLabelName() {
            string name = $"Label{nextLabel}";
            checked {
                nextLabel++;
            }
            return name;
        }

        private IEnumerable<ASM> Compile(ASTNode root) {
            switch(root.Type) {
                case ASTType.StatementBlock: return CompileStatementBlock(root);
                case ASTType.DeclarationStatement: return CompileDeclarationStatement(root);
                case ASTType.ExpressionAssignmentStatement: return CompileExpressionAssignmentStatement(root);
                case ASTType.ExpressionOperator: return CompileExpressionOperator(root);
                case ASTType.NumberLiteral:
                    ASM num = new();
                    num.Op = OpCode.load;
                    num.Arg0 = root.Value;
                    num.Arg1 = GetTargetRegisterName(targetRegister);
                    targetRegister = new();
                    return Enumerable.Repeat(num, 1);
                case ASTType.BoolLiteral:
                    if (root.Value != "false" && root.Value != "true") throw new Exception("Invalid bool");
                    ASM boolean = new();
                    boolean.Op = OpCode.load;
                    boolean.Arg0 = (root.Value == "true") ? "1" : "0"; //0 or 1
                    boolean.Arg1 = GetTargetRegisterName(targetRegister);
                    targetRegister = new();
                    return Enumerable.Repeat(boolean, 1);
                case ASTType.ExpressionIdentifier:
                    ASM ident = new();
                    ident.Op = OpCode.load;
                    ident.Arg0 = GetTargetRegisterName(new TargetRegister(root.Value));
                    ident.Arg1 = GetTargetRegisterName(targetRegister);
                    targetRegister = new();
                    return Enumerable.Repeat(ident, 1);
                case ASTType.Comment:
                    ASM comment = new();
                    comment.Comment = root.Value;
                    return Enumerable.Repeat(comment, 1);
                case ASTType.IterationStatement: return CompileIterationStatement(root);
                case ASTType.SelectionStatement: return CompileSelectionStatement(root);
                //ExpressionIndexer,
                default:
                    throw new Exception("Unknown operation.");
            }
        }

        private IEnumerable<ASM> CompileStatementBlock(ASTNode root) {
            foreach(var node in root.Params) {
                foreach(ASM asm in Compile(node)) {
                    yield return asm;
                }
            }
        }

        private IEnumerable<ASM> CompileDeclarationStatement(ASTNode root) {
            string name = root.Params[0].Value;
            registers[name] = GetUnusedRegister();

            if (root.Params.Count > 1) {
                return Compile(root.Params[1]);
            } else {
                return Enumerable.Empty<ASM>();
            }
        }

        private IEnumerable<ASM> CompileExpressionAssignmentStatement(ASTNode root) {
            string name = root.Params[0].Value;
            targetRegister = new(name);
            return Compile(root.Params[1]); // Value should be saved to target register
        }

        private IEnumerable<ASM> CompileExpressionOperator(ASTNode root) {
            ASM instruction = new();
            switch (root.Value) {
                case "+": instruction.Op = OpCode.add; break;
                case "-": instruction.Op = OpCode.sub; break;
                case "*": instruction.Op = OpCode.mul; break;
                case "/": instruction.Op = OpCode.div; break;
                case "and":
                case "&":
                    instruction.Op = OpCode.and; 
                    break;
                case "or":
                case "|":
                    instruction.Op = OpCode.or; 
                    break;
                case "xor":
                case "^":
                    instruction.Op = OpCode.xor; 
                    break;
                case "not":
                case "~": 
                    instruction.Op = OpCode.not; 
                    break;
                case "%": instruction.Op = OpCode.mod; break;
                case ">>": instruction.Op = OpCode.rsh; break;
                case "<<": instruction.Op = OpCode.lsh; break;
                //case ">":
                //case "<":
                //case "<=":
                //case ">=":
                //case "==":
                default: throw new Exception("Invalid operation");
            }

            TargetRegister target = targetRegister;
            List<int> borrowedRegisters = new();

            // Get first arg
            if (root.Params[0].Type == ASTType.NumberLiteral) {
                instruction.Arg0 = root.Params[0].Value;
            } else if (root.Params[0].Type == ASTType.ExpressionIdentifier) {
                instruction.Arg0 = GetTargetRegisterName(new (root.Params[0].Value));
            } else if (root.Params[0].Type == ASTType.ExpressionOperator) {
                int register = GetUnusedRegister();
                borrowedRegisters.Add(register);
                targetRegister = new(register);
                instruction.Arg0 = GetTargetRegisterName(targetRegister);
                foreach(var asm in Compile(root.Params[0])) {
                    yield return asm;
                }
            } else {
                throw new Exception("Invalid argument.");
            }

            // Get second arg, if applicable
            if (instruction.Op != OpCode.not) {
                if (root.Params[1].Type == ASTType.NumberLiteral) {
                    instruction.Arg1 = root.Params[1].Value;
                } else if (root.Params[1].Type == ASTType.ExpressionIdentifier) {
                    instruction.Arg1 = GetTargetRegisterName(new(root.Params[1].Value));
                } else if (root.Params[1].Type == ASTType.ExpressionOperator) {
                    int register = GetUnusedRegister();
                    borrowedRegisters.Add(register);
                    targetRegister = new(register);
                    instruction.Arg1 = GetTargetRegisterName(targetRegister);
                    foreach (var asm in Compile(root.Params[1])) {
                        yield return asm;
                    }
                } else {
                    throw new Exception("Invalid argument.");
                }

                instruction.Arg2 = GetTargetRegisterName(target);
            } else {
                instruction.Arg1 = GetTargetRegisterName(target);
            }

            foreach(int register in borrowedRegisters) {
                registersUsed[register] = false;
            }

            yield return instruction;
        }

        private IEnumerable<ASM> CompileIterationStatement(ASTNode root) {
            if (root.Type != ASTType.IterationStatement || root.Value != "while") throw new Exception("Invalid iteration type.");

            // TODO see SelectionStatement to support other types
            if (root.Params[0].Type != ASTType.BoolLiteral) throw new Exception("Unsupported condition type");

            if (root.Params[0].Value == "false") {
                // never runs, dont add any instructions from the statement block
                yield break;
            }

            if (root.Params[0].Value != "true") throw new Exception("Invalid bool type.");

            ASM label = new();
            label.Label = GetLabelName();
            yield return label;

            foreach(var instruction in CompileStatementBlock(root.Params[1])) {
                yield return instruction;
            }

            ASM jump = new();
            jump.Op = OpCode.jmp;
            jump.Arg0 = label.Label;
            yield return jump;
        }
        /*
        private OpCode GetOppositeJmp(OpCode jmp) {
            switch(jmp) {
                case OpCode.jmp_eq: return OpCode.jmp_neq;
                case OpCode.jmp_neq: return OpCode.jmp_eq;
                case OpCode.jmp_gt: return OpCode.jmp_lte;
                case OpCode.jmp_gte: return OpCode.jmp_lt;
                case OpCode.jmp_lt: return OpCode.jmp_gte;
                case OpCode.jmp_lte: return OpCode.jmp_gt;
                default: throw new Exception("Invalid jump operation");
            }
        }*/ 

        private IEnumerable<ASM> CompileSelectionStatement(ASTNode root) {
            if (root.Type != ASTType.SelectionStatement || root.Value != "if") throw new Exception("Invalid selection type.");


            if (root.Params[0].Type == ASTType.BoolLiteral) {
                if (root.Params[0].Value == "true") {
                    foreach (var instruct in CompileStatementBlock(root.Params[1])) {
                        yield return instruct;
                    }
                    yield break;
                } else if (root.Params[0].Value == "false") {
                    // Only applies for "else" or "else if" conditions.
                    if (root.Params.Count > 2) {
                        if (root.Params[2].Type == ASTType.SelectionStatement) {
                            foreach (var instruct in CompileSelectionStatement(root.Params[2])) {
                                yield return instruct;
                            }
                        } else {
                            foreach (var instruct in CompileStatementBlock(root.Params[2])) {
                                yield return instruct;
                            }
                        }
                    }
                    yield break;
                } else {
                    throw new Exception("Invalid bool");
                }
            }

            // Must be a bool type
            string exitLabelName = GetLabelName();

            if (root.Params[0].Type == ASTType.ExpressionIdentifier) {
                ASM jmp = new();
                jmp.Op = OpCode.jmp_eq;
                jmp.Arg0 = GetTargetRegisterName(new TargetRegister(root.Params[0].Value));
                jmp.Arg1 = "0";
                jmp.Arg2 = exitLabelName;
                yield return jmp;
            } else if (root.Params[0].Type == ASTType.ExpressionOperator) {
                // TODO in certain cases, the jmp statement can be simplified / done in less instructions
                ASM jmp = new();
                jmp.Arg2 = exitLabelName;

                bool evaluateBool = false;
                switch(root.Params[0].Value) {
                    case "and":
                    case "or":
                    case "xor":
                        evaluateBool = true;
                        jmp.Op = OpCode.jmp_eq;
                        jmp.Arg1 = "0";
                        break;
                    case "not":
                        evaluateBool = true;
                        jmp.Op = OpCode.jmp_neq;
                        jmp.Arg1 = "0";
                        break;
                    case "<=": jmp.Op = OpCode.jmp_gt; break;
                    case ">=": jmp.Op = OpCode.jmp_lt; break;
                    case "<": jmp.Op = OpCode.jmp_gte; break;
                    case ">": jmp.Op = OpCode.jmp_lte; break;
                    case "==": jmp.Op = OpCode.jmp_neq; break;
                    case "!=": jmp.Op = OpCode.jmp_eq; break;
                    default:
                        throw new Exception("Expected bool operator");
                }

                List<int> borrowedRegisters = new();
                if (root.Params[0].Value == "not") {
                    ASTNode shortcut = root.Params[0].Params[0];
                    // arg1
                    if (shortcut.Type == ASTType.NumberLiteral) { // TODO took this from CompileOperator. Create function?
                        jmp.Arg0 = shortcut.Value;
                    } else if (shortcut.Type == ASTType.ExpressionIdentifier) {
                        jmp.Arg0 = GetTargetRegisterName(new(shortcut.Value));
                    } else if (shortcut.Type == ASTType.ExpressionOperator) {
                        int register = GetUnusedRegister();
                        borrowedRegisters.Add(register);
                        targetRegister = new(register);
                        jmp.Arg0 = GetTargetRegisterName(targetRegister);
                        foreach (var asm in Compile(shortcut)) {
                            yield return asm;
                        }
                    } else {
                        throw new Exception("Invalid argument.");
                    }
                } else if (evaluateBool) {
                    // Evaluate arg1 into a register, then evaluate
                    int argRegister = GetUnusedRegister();
                    borrowedRegisters.Add(argRegister);
                    targetRegister = new(argRegister);
                    jmp.Arg0 = GetTargetRegisterName(targetRegister);
                    foreach(var instruct in CompileExpressionOperator(root.Params[0])) {
                        yield return instruct;
                    }
                } else {
                    // Arg1
                    ASTNode shortcut = root.Params[0].Params[0];
                    if (shortcut.Type == ASTType.NumberLiteral) {
                        jmp.Arg0 = root.Params[0].Value;
                    } else if (shortcut.Type == ASTType.ExpressionIdentifier) {
                        jmp.Arg0 = GetTargetRegisterName(new(shortcut.Value));
                    } else if (shortcut.Type == ASTType.ExpressionOperator) {
                        int register = GetUnusedRegister();
                        borrowedRegisters.Add(register);
                        targetRegister = new(register);
                        jmp.Arg0 = GetTargetRegisterName(targetRegister);
                        foreach (var asm in Compile(shortcut)) {
                            yield return asm;
                        }
                    } else if (shortcut.Type == ASTType.BoolLiteral) {
                        jmp.Arg0 = (shortcut.Value == "true") ? "1" : "0";
                    } else {
                        throw new Exception("Invalid argument.");
                    }

                    // Arg2
                    shortcut = root.Params[0].Params[1];
                    if (shortcut.Type == ASTType.NumberLiteral) {
                        jmp.Arg1 = shortcut.Value;
                    } else if (shortcut.Type == ASTType.ExpressionIdentifier) {
                        jmp.Arg1 = GetTargetRegisterName(new(shortcut.Value));
                    } else if (shortcut.Type == ASTType.ExpressionOperator) {
                        int register = GetUnusedRegister();
                        borrowedRegisters.Add(register);
                        targetRegister = new(register);
                        jmp.Arg1 = GetTargetRegisterName(targetRegister);
                        foreach (var asm in Compile(shortcut)) {
                            yield return asm;
                        }
                    } else if (shortcut.Type == ASTType.BoolLiteral) {
                        jmp.Arg1 = (shortcut.Value == "true") ? "1" : "0";
                    } else {
                        throw new Exception("Invalid argument.");
                    }
                }

                foreach(int reg in borrowedRegisters) {
                    registersUsed[reg] = false;
                }
                yield return jmp;
            } else {
                throw new Exception("Invalid condition");
            }

            foreach(var instruct in CompileStatementBlock(root.Params[1])) {
                yield return instruct;
            }

            if (root.Params.Count > 2) { // Else or else if
                string label2Name = GetLabelName();

                ASM jump2 = new();
                jump2.Op = OpCode.jmp;
                jump2.Arg0 = label2Name;
                yield return jump2;

                ASM label1 = new();
                label1.Label = exitLabelName;
                yield return label1;

                // statement block
                IEnumerable<ASM> block;
                if (root.Params[2].Type == ASTType.SelectionStatement) {
                    block = CompileSelectionStatement(root.Params[2]);
                } else {
                    block = CompileStatementBlock(root.Params[2]);
                }
                foreach(var instruct in block) {
                    yield return instruct;
                }

                exitLabelName = label2Name;
            }

            ASM exitLabel = new();
            exitLabel.Label = exitLabelName;
            yield return exitLabel;
            yield break;

        }
    }
}
