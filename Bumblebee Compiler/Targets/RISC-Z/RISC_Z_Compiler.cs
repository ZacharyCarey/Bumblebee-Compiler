using System;
using System.Collections.Generic;
using System.Linq;
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
            jmp = 0b10000
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
            if (root.Value != "while") throw new Exception("Invalid iteration type.");

            if (root.Params[0].Type != ASTType.BoolLiteral) throw new Exception("Unsupported condition type");

            if (root.Params[0].Value == "false") {
                // never runs, dont add any instructions from the statement block
                yield break;
            }

            if (root.Params[0].Value != "true") throw new Exception("Invalid bool type.");

            ASM label = new();
            label.Label = $"Label{nextLabel}";
            nextLabel++;
            yield return label;

            foreach(var instruction in CompileStatementBlock(root.Params[1])) {
                yield return instruction;
            }

            ASM jump = new();
            jump.Op = OpCode.jmp;
            jump.Arg0 = label.Label;
            yield return jump;
        }
    }
}
