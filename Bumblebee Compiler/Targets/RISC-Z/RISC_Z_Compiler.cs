//using Microsoft.Win32;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection.Emit;
//using System.Security.Cryptography;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml.Linq;
//using static Bumblebee_Compiler.Targets.RISC_Z.RISC_Z_Registers;

//namespace Bumblebee_Compiler.Targets.RISC_Z {
//    internal class RISC_Z_Registers {
//        internal struct TargetRegister {
//            public int number = -1;
//            public string name = null;
//            public bool IsNewRegister = false;
//            public TargetRegister(int reg, bool newRegister) {
//                this.number = reg;
//                this.IsNewRegister = newRegister;
//            }
//            public TargetRegister(string name) {
//                this.name = name;
//            }
//            public TargetRegister() { }

//            public override string ToString() {
//                if (number >= 0 && name != null) throw new Exception("Invalid register state.");
//                if (number >= 0) {
//                    return "reg" + number;
//                }  else {
//                    return name;
//                }
//            }
//        }

//        Dictionary<string, TypeName> functions = new();
//        bool[] registersUsed = new bool[20];
//        Dictionary<string, int> registers = new();
//        TargetRegister targetRegister = new();
//        uint nextLabel = 0;
//        Stack<List<object>> context = new();
//        Dictionary<string, string> consts = new();
//        Dictionary<string, int> ArrayAddress = new();
//        Dictionary<string, int> ArraySize = new();
//        int nextRAMLocation = 0;

//        public RISC_Z_Registers (Dictionary<string, VariableOptions> variables, Dictionary<string, FunctionOptions> functions) {
//            foreach (var pair in variables) {
//                this.registers.Add(pair.Key, -1);
//            }
//            foreach(var pair in functions) {
//                this.functions.Add(pair.Key, pair.Value.ReturnType);
//            }
//        }

//        public TargetRegister CreateRegister() {
//            for (int i = 0; i < registersUsed.Length; i++) {
//                if (registersUsed[i] == false) {
//                    registersUsed[i] = true;
//                    context.Peek().Add(i);
//                    return new TargetRegister(i, true);
//                }
//            }
//            throw new Exception("No remaining registers available.");
//        }

//        public TargetRegister CreateRegister(string varName) {
//            switch (varName) {
//                case "input":
//                case "output":
//                case "HI":
//                case "counter":
//                case "stack":
//                    throw new Exception("Can not create register for known-register");
//            }

//            if (registers.ContainsKey(varName)) throw new Exception("Variable already exists");
//            if (consts.ContainsKey(varName)) throw new Exception("Variable already exists");
//            if (functions.ContainsKey(varName)) throw new Exception("Function name already exists.");

//            TargetRegister newReg = CreateRegister();
//            registers[varName] = newReg.number;
//            context.Peek().Add(varName);
//            return newReg;
//        }

//        public TargetRegister CreateArray(string name, int size) {
//            switch(name) {
//                case "input":
//                case "output":
//                case "HI":
//                case "counter":
//                case "stack":
//                    throw new Exception("Can not create array for known-register");
//            }

//            if (registers.ContainsKey(name)) throw new Exception("Variable already exists");
//            if (consts.ContainsKey(name)) throw new Exception("Variable already exists");
//            if (functions.ContainsKey(name)) throw new Exception("Function name already exists");
//            if (ArrayAddress.ContainsKey(name)) throw new Exception("Array already exists.");

//            int addr = nextRAMLocation;
//            nextRAMLocation += size;
//            ArrayAddress.Add(name, addr);
//            ArraySize.Add(name, size);
//            context.Peek().Add(name);
//            return new TargetRegister(addr.ToString());
//        }

//        public TargetRegister GetArray(string name) {
//            return new TargetRegister(ArrayAddress[name].ToString());
//        }

//        public int GetArrayLength(string name) {
//            return ArraySize[name];
//        }

//        public TargetRegister GetRegister(string varName) {
//            switch (varName) {
//                case "input":
//                case "output":
//                case "HI":
//                case "counter":
//                case "stack":
//                    return new TargetRegister(varName);
//            }

//            if (registers.ContainsKey(varName)) {
//                int register = registers[varName];
//                if (register < -1) throw new Exception("Tried to read predefined variable name");
//                return new TargetRegister(register, false);
//            } else if (consts.ContainsKey(varName)) {
//                return new TargetRegister(consts[varName]);
//            } else {
//                throw new Exception("Can't find variable name");
//            }
//        }

//        public void RemoveRegister(TargetRegister register) {
//            if (register.number < 0) throw new Exception("Not a returnable register");
//            registersUsed[register.number] = false;
//        }

//        public void RemoveRegister(string varName) {
//            int reg = registers[varName];
//            registers.Remove(varName);
//            registersUsed[reg] = false;
//        }

//        public void RemoveArray(string name) {
//            ArrayAddress.Remove(name);
//            ArraySize.Remove(name);
//        }

//        public string GetLabelName() {
//            string name = $"Label{nextLabel}";
//            checked {
//                nextLabel++;
//            }
//            return name;
//        }

//        public void CreateContext() {
//            context.Push(new());
//        }

//        public void RemoveContext() {
//            var variables = context.Pop();
//            foreach(var variable in variables) {
//                if (variable is int registerNumber) {
//                    registersUsed[registerNumber] = false;
//                } else if (variable is string variableName) {
//                    if (consts.ContainsKey(variableName)) consts.Remove(variableName);
//                    if (registers.ContainsKey(variableName)) registers.Remove(variableName);
//                    ArrayAddress.Remove(variableName);
//                    ArraySize.Remove(variableName);
//                } else {
//                    throw new Exception("Unknown register type");
//                }
//            }
//        }

//        public IEnumerable<int> GetContextDifference(int targetContext) {
//            foreach(var currentContext in context.Reverse().Zip(Enumerable.Range(0, context.Count))){
//                if (currentContext.Second < targetContext) continue;
//                foreach(var register in currentContext.First) {
//                    if (register is int registerNumber) {
//                        yield return registerNumber;
//                    }
//                }
//            }
//        }

//        public void AddConst(string name, string value) {
//            if (registers.ContainsKey(name)) throw new Exception("Variable already exists");
//            if (consts.ContainsKey(name)) throw new Exception("Variable already exists");
//            if (functions.ContainsKey(name)) throw new Exception("Function already exists.");
//            if (ArrayAddress.ContainsKey(name)) throw new Exception("Array already exists");
//            consts.Add(name, value);
//            context.Peek().Add(name);
//        }

//        public void AddFunction(string name, string returnType) {
//            if (registers.ContainsKey(name)) throw new Exception("Variable already exists");
//            if (consts.ContainsKey(name)) throw new Exception("Variable already exists");
//            if (functions.ContainsKey(name)) throw new Exception("Function already exists");
//            if (ArrayAddress.ContainsKey(name)) throw new Exception("Array already exists");
//            functions.Add(name, new TypeName(returnType));
//        }

//        public TypeName GetFunctionReturnType(string name) {
//            TypeName type = functions[name];
//            return type;
//        }
//    }

//    internal class RISC_Z_Compiler : ICompiler {
//        public Dictionary<string, VariableOptions> KnownVariables => new(){
//            {"input", new() {IsReadable = true, IsWritable = false, TypeName = new TypeName("uint8")} },
//            {"output", new() {IsReadable = false, IsWritable = true, TypeName = new TypeName("uint8")} },
//            {"HI", new() {IsReadable = true, IsWritable = true, TypeName = new TypeName("uint8")} },
//            {"counter", new() {IsReadable = true, IsWritable = true, TypeName = new TypeName("uint8")} },
//            {"stack", new() {IsReadable = true, IsWritable = true, TypeName = new TypeName("uint8")} }
//        };

//        public Dictionary<string, FunctionOptions> KnownFunctions => new(){ };

//        StreamWriter writer = null;
//        RISC_Z_Registers registers = null;

//        public void Compile(Parser program, StreamWriter outputFile) {
//            writer = outputFile;
//            registers = new(KnownVariables, KnownFunctions);

//            CompileFunctionDefinitions(program.Functions);

//            registers.CreateContext();
//            foreach(var instruction in CompileStatementBlock(program.AST, false)) {
//                outputFile.WriteLine(instruction.ToString());
//            }

//            ASM reset = new();
//            reset.Op = OpCode.jmp;
//            reset.Arg0 = "0";
//            outputFile.WriteLine(reset.ToString());
//            outputFile.WriteLine();

//            foreach(var instruction in CompileFunctionImplementations(program.Functions)) {
//                outputFile.WriteLine(instruction.ToString());
//            }
//        }

//        internal enum OpCode {
//            add = 0b0,
//            sub = 0b1,
//            and = 0b10,
//            or = 0b11,
//            xor = 0b100,
//            not = 0b101,
//            lsh = 0b110,
//            rsh = 0b111,
//            mul = 0b1000,
//            div = 0b1001,
//            mod = 0b1010,
//            load = 0b11111,
//            jmp = 0b10000,
//            jmp_eq = 0b10001,
//            jmp_neq = 0b10010,
//            jmp_gt = 0b10011,
//            jmp_gte = 0b10100,
//            jmp_lt = 0b10101,
//            jmp_lte = 0b10110,
//            call = 0b11000,
//            ret = 0b11001,
//            rram = 0b11010, // addr, index, dst
//            wram = 0b11011 // addr, index, arg2
//        }

//        struct ASM : IEnumerable<ASM> {
//            /*public bool LoadArg0 = false;
//            public bool LoadArg1 = false;
//            public bool LoadArg2 = false;*/
//            public OpCode Op;
//            public string Arg0;
//            public string Arg1;
//            public string Arg2;
//            public string Comment = null;
//            public string Label = null;

//            public ASM() { }

//            public override string ToString() {
//                if (Comment != null) {
//                    if (Comment != "") {
//                        return "# " + Comment;
//                    } else {
//                        return Comment;
//                    }
//                }
//                if (Label != null) return "label " + Label;

//                string asm = Op.ToString();
//                string arg0 = Arg0;
//                string arg1 = Arg1;
//                string arg2 = Arg2;
//                int temp;
//                bool isRam = (Op == OpCode.wram || Op == OpCode.rram);
//                if (int.TryParse(Arg0, out temp) ^ isRam) {
//                    asm += "|arg0";
//                    arg0 = Arg0;
//                }
//                if (int.TryParse(Arg1, out temp) ^ isRam) {
//                    asm += "|arg1";
//                    arg1 = Arg1;
//                }
//                if (int.TryParse(Arg2, out temp)) {
//                    asm += "|arg2";
//                    arg2 = Arg2;
//                }

//                switch (Op) {
//                    case OpCode.add:
//                    case OpCode.sub:
//                    case OpCode.and:
//                    case OpCode.or:
//                    case OpCode.xor:
//                    case OpCode.lsh:
//                    case OpCode.rsh:
//                    case OpCode.mul:
//                    case OpCode.div:
//                    case OpCode.mod:
//                    case OpCode.jmp_eq:
//                    case OpCode.jmp_neq:
//                    case OpCode.jmp_gt:
//                    case OpCode.jmp_gte:
//                    case OpCode.jmp_lt:
//                    case OpCode.jmp_lte:
//                    case OpCode.wram:
//                    case OpCode.rram:
//                        asm += " " + arg0;
//                        asm += " " + arg1;
//                        asm += " " + arg2;
//                        return asm;
//                    case OpCode.not:
//                    case OpCode.load:
//                    case OpCode.call:
//                        asm += " " + arg0;
//                        asm += " " + arg1;
//                        return asm;
//                    case OpCode.jmp:
//                        asm += " " + arg0;
//                        return asm;
//                    case OpCode.ret:
//                        return asm;
//                    default:
//                        throw new Exception("Unknown opcode.");
//                }
//            }

//            private class ASMIterator : IEnumerator<ASM> {
//                private ASM value;
//                bool init = true;
//                public ASM Current => value;
//                object IEnumerator.Current => value;

//                public ASMIterator(ASM value) {
//                    this.value = value;
//                }

//                public void Dispose() { }

//                public bool MoveNext() {
//                    if (init) {
//                        init = false;
//                        return true;
//                    } else {
//                        return false;
//                    }
//                }

//                public void Reset() {
//                    init = true;
//                }
//            }

//            public IEnumerator<ASM> GetEnumerator() {
//                return new ASMIterator(this);
//            }
//            IEnumerator IEnumerable.GetEnumerator() {
//                return new ASMIterator(this);
//            }
//        }

//        private void CompileFunctionDefinitions(List<ASTNode> functions) {
//            foreach(ASTNode function in functions) {
//                if (function.Params[0].Type != ASTType.ExpressionIdentifier) throw new Exception("Expected function name");
//                string name = function.Params[0].Value;
//                registers.AddFunction(name, function.Value);
//            }
//        }

//        private IEnumerable<ASM> CompileFunctionImplementations(List<ASTNode> functions) {
//            // TODO something needs to check for return statements
//            foreach (ASTNode function in functions) {
//                registers.CreateContext();

//                ASM func_label = new();
//                func_label.Label = function.Params[0].Value;
//                yield return func_label;

//                // Pop parameters from the stack in reverse order
//                foreach(ASTNode argument in function.Params.Skip(2).Reverse()) {
//                    if (argument.Type != ASTType.FunctionArgument) throw new Exception("Expected function argument");
//                    if (argument.Params[0].Type != ASTType.ExpressionIdentifier) throw new Exception("Expected argument name");
//                    string argumentName = argument.Params[0].Value;
//                    TargetRegister register = registers.CreateRegister(argumentName);

//                    ASM load = new();
//                    load.Op = OpCode.load;
//                    load.Arg0 = "stack";
//                    load.Arg1 = register.ToString();
//                    yield return load;
//                }

//                // Compile body
//                foreach(var instruction in CompileStatementBlock(function.Params[1])) {
//                    yield return instruction;
//                }

//                registers.RemoveContext();

//                // TODO cant return on a non-void function
//                ASM ret = new();
//                ret.Op = OpCode.ret;
//                yield return ret;

//                ASM emptyLine = new();
//                emptyLine.Comment = "";
//                yield return emptyLine;
//            }
//        }

//        private IEnumerable<ASM> CompileFunctionCall(ASTNode node, ref TargetRegister? target) {
//            if (node.Type != ASTType.FunctionCall) throw new Exception("Expected function call");
//            TypeName returnType = registers.GetFunctionReturnType(node.Value);

//            // Find which registers need to be stored
//            List<string> callRegisters = new();
//            HashSet<int> seenRegister = new();
//            foreach(int register in registers.GetContextDifference(1)) {
//                if (seenRegister.Add(register)) {
//                    if (register >= 8) throw new Exception($"Unable to store register {register} for function call.");
//                    callRegisters.Add("creg" + register);
//                }
//            }

//            IEnumerable<ASM> instructions = Enumerable.Empty<ASM>();

//            // Load argument onto the stack
//            foreach (var argument in node.Params) {
//                TargetRegister? stack = new TargetRegister("stack");
//                instructions = instructions.Concat(CompileExpression(argument, ref stack));
//                if (stack != null) {
//                    ASM load = new();
//                    load.Op = OpCode.load;
//                    load.Arg0 = stack.ToString();
//                    load.Arg1 = "stack";
//                    instructions = instructions.Concat(load);
//                }
//            }

//            // Call function
//            ASM call = new();
//            call.Op = OpCode.call;
//            call.Arg0 = node.Value;
//            if (callRegisters.Count == 0) {
//                call.Arg1 = "none";
//            } else {
//                call.Arg1 = string.Join("|", callRegisters);
//            }

//            instructions = instructions.Concat(call);

//            // pop return value from stack (optional)
//            if (returnType != new TypeName("void")) {
//                if (target != null) {
//                    ASM load = new();
//                    load.Op = OpCode.load;
//                    load.Arg0 = "stack";
//                    load.Arg1 = target.ToString();
//                    instructions = instructions.Concat(load);
//                } else {
//                    target = new TargetRegister("stack");
//                }
//            } else {
//                if (target != null) {
//                    throw new Exception("Can't return value from void type function");
//                }
//            }

//            return instructions;
//        }

//        private IEnumerable<ASM> CompileFunctionReturn(ASTNode node) {
//            if (node.Type != ASTType.FunctionReturn) throw new Exception("Expected function return");

//            IEnumerable<ASM> instructions = Enumerable.Empty<ASM>();

//            if (node.Params.Count > 0) {
//                // load return value into the stack
//                TargetRegister? target = new TargetRegister("stack");
//                instructions = instructions.Concat(CompileExpression(node.Params[0], ref target));
//                if (target != null) {
//                    ASM load = new();
//                    load.Op = OpCode.load;
//                    load.Arg0 = target.ToString();
//                    load.Arg1 = "stack";
//                    instructions = instructions.Concat(load);
//                }
//            }

//            ASM ret = new();
//            ret.Op = OpCode.ret;
//            instructions = instructions.Concat(ret);

//            return instructions;
//        }

//        private IEnumerable<ASM> CompileComment(ASTNode node) {
//            // TODO comments save a blank line
//            if (node.Type != ASTType.Comment) throw new Exception("Expected comment.");
//            ASM asm = new ASM();
//            asm.Comment = node.Value;
//            yield return asm;
//        } 

//        private void CompileNumberLiteral(ASTNode node, out TargetRegister target) {
//            if (node.Type != ASTType.NumberLiteral) throw new Exception("Expected number literal.");
//            target = new TargetRegister(node.Value);
//        }
        
//        private void CompileBoolLiteral(ASTNode node, out TargetRegister target) {
//            if (node.Type != ASTType.BoolLiteral) throw new Exception("Expected bool literal.");
//            if (node.Value == "true") {
//                target = new TargetRegister("1");
//            } else if (node.Value == "false") {
//                target = new TargetRegister("0");
//            } else {
//                throw new Exception("Invalid bool value.");
//            }
//        }

//        /// <summary>
//        /// If target is provided (not null), then the resulting operation will be stores in that register
//        /// IF POSSIBLE and set the target to null. Some operation, like a NumberLiteral, 
//        /// does not do an operation. In these cases, target will be set to the register that
//        /// should be used.
//        /// 
//        /// In short, after calling if target==null then the value is already stored in the given target.
//        /// if target!=null, then that register should be used as the argument.
//        /// 
//        /// See "CompileExpressionAssignmentStatement" as a simple example
//        /// </summary>
//        private IEnumerable<ASM> CompileExpression(ASTNode node, ref TargetRegister? target) {
//            TargetRegister result;
//            if (node.Type == ASTType.FunctionCall) {
//                return CompileFunctionCall(node, ref target);
//            } else if (node.Type == ASTType.NumberLiteral) {
//                CompileNumberLiteral(node, out result);
//                target = result;
//                return Enumerable.Empty<ASM>();
//            } else if (node.Type == ASTType.BoolLiteral) {
//                CompileBoolLiteral(node, out result);
//                target = result;
//                return Enumerable.Empty<ASM>();
//            } else if (node.Type == ASTType.ExpressionIdentifier) {
//                target = registers.GetRegister(node.Value);
//                return Enumerable.Empty<ASM>();
//            } else if (node.Type == ASTType.ExpressionOperator) {
//                return CompileExpressionOperator(node, ref target);
//            } else if (node.Type == ASTType.ExpressionIndexer) {
//                return CompileIndexer(node, ref target);
//            } else if (node.Type == ASTType.Accessor) {
//                return CompileAccessor(node, ref target);
//            } else if (node.Type == ASTType.ExpressionAssignmentStatement) {
//                return CompileExpressionAssignmentStatement(node);
//            } else {
//                throw new Exception("Invalid argument.");
//            }
//        }

//        private IEnumerable<ASM> CompileAccessor(ASTNode node, ref TargetRegister? target) {
//            if (node.Type != ASTType.Accessor) throw new Exception("Expected accessor");
//            if (node.Value != "Length") throw new Exception("Unknown accessor");
//            if (node.Params[0].Type != ASTType.ExpressionIdentifier) throw new Exception("Expected identifier");
//            string name = node.Params[0].Value;
//            int len = registers.GetArrayLength(name);
//            target = new TargetRegister(len.ToString());
//            return Enumerable.Empty<ASM>();
//        }

//        /// <summary>
//        /// See "CompileExpression" on how argument "target" should be handled.
//        /// </summary>
//        private IEnumerable<ASM> CompileExpressionOperator(ASTNode node, ref TargetRegister? target) {
//            if (node.Type != ASTType.ExpressionOperator) throw new Exception("Expected operator.");

//            ASM asm = new();
//            IEnumerable<ASM> instructions = Enumerable.Empty<ASM>();
//            switch (node.Value) {
//                case "+": asm.Op = OpCode.add; break;
//                case "-": asm.Op = OpCode.sub; break;
//                case "*": asm.Op = OpCode.mul; break;
//                case "/": asm.Op = OpCode.div; break;
//                case "and":
//                case "&":
//                    asm.Op = OpCode.and;
//                    break;
//                case "or":
//                case "|":
//                    asm.Op = OpCode.or;
//                    break;
//                case "xor":
//                case "^":
//                    asm.Op = OpCode.xor;
//                    break;
//                case "not":
//                case "~":
//                    asm.Op = OpCode.not;
//                    break;
//                case "%": asm.Op = OpCode.mod; break;
//                case ">>": asm.Op = OpCode.rsh; break;
//                case "<<": asm.Op = OpCode.lsh; break;
//                //case ">":
//                //case "<":
//                //case "<=":
//                //case ">=":
//                //case "==":
//                default: throw new Exception("Invalid operation");
//            }

//            List<TargetRegister> usedRegisters = new();

//            // Get first arg
//            TargetRegister? arg0 = null;
//            instructions = instructions.Concat(CompileExpression(node.Params[0], ref arg0));
//            if (arg0 == null) throw new Exception("Expected a return register");
//            if (((TargetRegister)arg0).IsNewRegister) {
//                usedRegisters.Add((TargetRegister)arg0);
//            }
//            asm.Arg0 = arg0.ToString();

//            // Get second arg, if applicable
//            ref string destinationArg = ref asm.Arg2;
//            if (asm.Op != OpCode.not) {
//                TargetRegister? arg1 = null;
//                instructions = instructions.Concat(CompileExpression(node.Params[1], ref arg1));
//                if (arg1 == null) throw new Exception("Expected a return register");
//                if (((TargetRegister)arg1).IsNewRegister) {
//                    usedRegisters.Add((TargetRegister)arg1);
//                }
//                asm.Arg1 = arg1.ToString();

//                destinationArg = ref asm.Arg2;
//            } else {
//                destinationArg = ref asm.Arg1;
//            }
//            // TODO order of operations?
//            foreach (TargetRegister register in usedRegisters) {
//                registers.RemoveRegister(register);
//            }

//            if (target == null) {
//                // Return the newly created register we are saving to
//                target = registers.CreateRegister();
//                destinationArg = target.ToString();
//            } else {
//                // return null to signify we did save to the given register
//                destinationArg = target.ToString();
//                target = null;
//            }

//            instructions = instructions.Concat(asm);
//            return instructions;
//        }

//        private IEnumerable<ASM> CompileStatementBlock(ASTNode node, bool createContext = true) {
//            if (node.Type != ASTType.StatementBlock) throw new Exception("Expected block statement.");

//            if (createContext) {
//                registers.CreateContext();
//            }
//            foreach(ASTNode statement in node.Params) {
//                IEnumerable<ASM> instructions;
//                if (statement.Type == ASTType.FunctionCall) {
//                    TargetRegister? register = null;
//                    instructions = CompileFunctionCall(statement, ref register);
//                    if (register != null && ((TargetRegister)register).IsNewRegister) {
//                        registers.RemoveRegister((TargetRegister)register);
//                    }
//                } else if (statement.Type == ASTType.FunctionReturn) {
//                    instructions = CompileFunctionReturn(statement);
//                } else if (statement.Type == ASTType.DeclarationStatement) {
//                    instructions = CompileDeclarationStatement(statement);
//                } else if (statement.Type == ASTType.ExpressionAssignmentStatement) {
//                    instructions = CompileExpressionAssignmentStatement(statement);
//                } else if (statement.Type == ASTType.StatementBlock) {
//                    instructions = CompileStatementBlock(statement);
//                } else if (statement.Type == ASTType.IterationStatement) {
//                    instructions = CompileIterationStatement(statement);
//                } else if (statement.Type == ASTType.SelectionStatement) {
//                    instructions = CompileSelectionStatement(statement);
//                } else if (statement.Type == ASTType.Comment) {
//                    instructions = CompileComment(statement);
//                } else {
//                    throw new Exception("Invalid statement type.");
//                }

//                foreach(ASM asm in instructions) {
//                    yield return asm;
//                }
//            }
//            if (createContext) {
//                registers.RemoveContext();
//            }
//        }

//        private IEnumerable<ASM> CompileIndexer(ASTNode node, ref TargetRegister? target) {
//            if (node.Type != ASTType.ExpressionIndexer) throw new Exception("Expected indexer");

//            string name = node.Value;
//            TargetRegister array = registers.GetArray(name);

//            TargetRegister? index = null;
//            IEnumerable<ASM> instructions = CompileExpression(node.Params[0], ref index);
//            if (index == null) throw new Exception("Expected index value");

//            ASM read = new();
//            read.Op = OpCode.rram;
//            read.Arg0 = array.ToString();
//            read.Arg1 = index.ToString();
//            if (target == null) {
//                target = registers.CreateRegister();
//                read.Arg2 = target.ToString();
//            } else {
//                read.Arg2 = target.ToString();
//                target = null;
//            }
//            instructions = instructions.Concat(read);

//            return instructions;
//        }

//        private IEnumerable<ASM> CompileExpressionAssignmentStatement(ASTNode node) {
//            if (node.Type != ASTType.ExpressionAssignmentStatement) throw new Exception("Expected ExpressionAssignmentStatement.");

//            if (node.Params[0].Type == ASTType.ExpressionIndexer) {
//                string name = node.Params[0].Value;
//                TargetRegister dest = registers.GetArray(name);

//                ASTNode indexer = node.Params[0].Params[0];
//                TargetRegister? index = null;
//                IEnumerable<ASM> instructions = CompileExpression(indexer, ref index);
//                if (index == null) throw new Exception("Expected index value");

//                TargetRegister? value = null;
//                instructions = instructions.Concat(CompileExpression(node.Params[1], ref value));
//                if (value == null) throw new Exception("Expected assignment value");

//                ASM write = new ASM();
//                write.Op = OpCode.wram;
//                write.Arg0 = dest.ToString();
//                write.Arg1 = index.ToString();
//                write.Arg2 = value.ToString();
//                instructions = instructions.Concat(write);

//                return instructions;
//            } else {
//                TargetRegister dest;
//                string name = node.Params[0].Value;
//                dest = registers.GetRegister(name);

//                TargetRegister? target = dest;
//                IEnumerable<ASM> instructions = CompileExpression(node.Params[1], ref target); // Value should be saved to target register
//                if (target != null) {
//                    // Result was not saved to the target, so we need to do a load operation
//                    ASM load = new ASM();
//                    load.Op = OpCode.load;
//                    load.Arg0 = target.ToString();
//                    load.Arg1 = dest.ToString();
//                    instructions = instructions.Concat(load);
//                }
//                return instructions;
//            }
//        }

//        // TODO somewhere needs to check for using uninitialized variable
//        private IEnumerable<ASM> CompileDeclarationStatement(ASTNode node) {
//            if (node.Type != ASTType.DeclarationStatement) throw new Exception("Expected DeclarationStatement.");

//            if (node.Params[0].Type != ASTType.ExpressionIdentifier) throw new Exception("Expected identifier.");
//            string name = node.Params[0].Value;

//            int index = 1;
//            bool isConst = false;
//            while (index < node.Params.Count && node.Params[index].Type == ASTType.VariableModifier) {
//                ref bool modifier = ref isConst;
//                switch (node.Params[index].Value) {
//                    case "const": modifier = ref isConst; break;
//                    default:
//                        throw new Exception("Unknown modifier");
//                }
//                if (modifier) throw new Exception("Modifier was listed twice");
//                modifier = true;
//                index++;
//            }

            
//            if (isConst) {
//                if (node.ValueType.IsArray) throw new Exception("Arrays can't be const");
//                if (index >= node.Params.Count) throw new Exception("Expected const value");

//                string value = null;
//                ASTNode constArg = node.Params[index].Params[1];
//                if (constArg.Type == ASTType.NumberLiteral) {
//                    value = constArg.Value;
//                } else if (constArg.Type == ASTType.BoolLiteral) {
//                    if (constArg.Value == "true") {
//                        value = "1";
//                    } else if (constArg.Value == "false") {
//                        value = "0";
//                    } else {
//                        throw new Exception("Invalid bool type");
//                    }
//                } else {
//                    throw new Exception("Invalid const type");
//                }
//                if (value == null) throw new Exception("Unexpected error.");
//                registers.AddConst(name, value);
//                return Enumerable.Empty<ASM>();
//            } else {
//                if (node.ValueType.IsArray) {
//                    registers.CreateArray(name, node.ValueType.ArrayLength);
//                    return Enumerable.Empty<ASM>();
//                } else {
//                    TargetRegister register = registers.CreateRegister(name);

//                    if (index < node.Params.Count) {
//                        return CompileExpressionAssignmentStatement(node.Params[index]);
//                    } else {
//                        return Enumerable.Empty<ASM>();
//                    }
//                }
//            }


//        }

//        private IEnumerable<ASM> CompileIterationStatement(ASTNode node) {
//            if (node.Type != ASTType.IterationStatement) throw new Exception("Expected iteration type.");
//            if (node.Value == "while") {
//                string else_label;
//                bool always_true;
//                bool always_false;
//                IEnumerable<ASM> instructions = GetJumpStatementFromCondition(node.Params[0], out else_label, out always_true, out always_false);

//                if (always_true) {
//                    // Since the statement is always true, always run block A with a single jump
//                    ASM loop_label = new();
//                    loop_label.Label = registers.GetLabelName();
//                    yield return loop_label;

//                    foreach (var asm in CompileStatementBlock(node.Params[1])) {
//                        yield return asm;
//                    }

//                    ASM true_jmp = new();
//                    true_jmp.Op = OpCode.jmp;
//                    true_jmp.Arg0 = loop_label.Label;
//                    yield return true_jmp;
//                } else if (always_false) {
//                    // If statement always false, there is nothing to run
//                    yield break;
//                }

//                ASM cond_label = new();
//                cond_label.Label = registers.GetLabelName();
//                yield return cond_label;

//                foreach (var asm in instructions) {
//                    yield return asm;
//                }

//                foreach (var asm in CompileStatementBlock(node.Params[1])) {
//                    yield return asm;
//                }

//                ASM jmp = new();
//                jmp.Op = OpCode.jmp;
//                jmp.Arg0 = cond_label.Label;
//                yield return jmp;

//                ASM else_label_asm = new();
//                else_label_asm.Label = else_label;
//                yield return else_label_asm;

//                yield break;
//            } else if (node.Value == "for") {
//                bool always_true;
//                bool always_false;
//                string exitLabelName;
                
//                registers.CreateContext();
//                if (node.Params[0] != null) {
//                    foreach (var asm in CompileDeclarationStatement(node.Params[0])) {
//                        yield return asm;
//                    }
//                }
//                IEnumerable<ASM> jumpAsm = GetJumpStatementFromCondition(node.Params[1], out exitLabelName, out always_true, out always_false);

//                if (always_false) {
//                    registers.RemoveContext();
//                    yield break;
//                }

//                ASM loopLabel = new();
//                loopLabel.Label = registers.GetLabelName();
//                yield return loopLabel;

//                if (!always_true) {
//                    foreach (var asm in jumpAsm) {
//                        yield return asm;
//                    }
//                }

//                foreach (var asm in CompileStatementBlock(node.Params[3], false)) {
//                    yield return asm;
//                }

//                if (node.Params[2] != null) {
//                    TargetRegister? target = null;
//                    foreach (var asm in CompileExpression(node.Params[2], ref target)) {
//                        yield return asm;
//                    }
//                }

//                ASM jump = new();
//                jump.Op = OpCode.jmp;
//                jump.Arg0 = loopLabel.Label;
//                yield return jump;

//                if (!always_true) {
//                    ASM exitLabel = new();
//                    exitLabel.Label = exitLabelName;
//                    yield return exitLabel;
//                }

//                registers.RemoveContext();

//                yield break;
//            } else {
//                throw new Exception("Unknown iteration type");
//            }
//        }

//        private IEnumerable<ASM> GetJumpStatementFromCondition(ASTNode node, out string exit_label, out bool alwaysTrue, out bool alwaysFalse) {
//            if (node.Type == ASTType.BoolLiteral) {
//                string boolean = node.Value;
//                if (boolean == "true") {
//                    // Since the statement is always true, always run block A without adding any jumps
//                    alwaysTrue = true;
//                    alwaysFalse = false;
//                    exit_label = null;
//                    return Enumerable.Empty<ASM>();
//                } else if (boolean == "false") {
//                    // Only applies for "else" or "else if" conditions.
//                    // If statement always false, always run block B without adding any jumps
//                    alwaysTrue = false;
//                    alwaysFalse = true;
//                    exit_label = null;
//                    return Enumerable.Empty<ASM>();
//                } else {
//                    throw new Exception("Invalid bool");
//                }
//            } else if (node.Type == ASTType.FunctionCall) {
//                TypeName returnType = registers.GetFunctionReturnType(node.Value);
//                if (returnType.Name != "bool" || returnType.IsArray) throw new Exception("Invalid result");
//                alwaysTrue = false;
//                alwaysFalse = false;
//                exit_label = registers.GetLabelName();

//                TargetRegister? target = null;
//                IEnumerable<ASM> instructions = CompileFunctionCall(node, ref target);
//                if (target == null) throw new Exception("Expected return value");
                
//                ASM jmp = new();
//                jmp.Op = OpCode.jmp_eq;
//                jmp.Arg0 = target.ToString();
//                jmp.Arg1 = "0";
//                jmp.Arg2 = exit_label;
//                return instructions.Concat(jmp);
//            }else if (node.Type == ASTType.ExpressionIdentifier) {
//                alwaysTrue = false;
//                alwaysFalse = false;
//                exit_label = registers.GetLabelName();
//                ASM jmp = new();
//                jmp.Op = OpCode.jmp_eq;
//                jmp.Arg0 = registers.GetRegister(node.Value).ToString();
//                jmp.Arg1 = "0";
//                jmp.Arg2 = exit_label;
//                return jmp;
//            } else if (node.Type == ASTType.ExpressionOperator) {
//                // TODO in certain cases, the jmp statement can be simplified / done in less instructions
//                alwaysFalse = false;
//                alwaysTrue = false;
//                exit_label = registers.GetLabelName();
//                ASM jmp = new();
//                jmp.Arg2 = exit_label;

//                switch (node.Value) {
//                    case "and":
//                    case "or":
//                    case "xor":
//                        jmp.Op = OpCode.jmp_eq;
//                        jmp.Arg1 = "0";
//                        break;
//                    case "not":
//                        ASTNode exp;
//                        if (node.Value == "not") {
//                            jmp.Op = OpCode.jmp_neq;
//                            exp = node.Params[0];
//                        } else {
//                            jmp.Op = OpCode.jmp_eq;
//                            exp = node;
//                        }
//                        jmp.Arg1 = "0";
//                        TargetRegister? arg = null;
//                        IEnumerable<ASM> instruct = CompileExpression(exp, ref arg);
//                        if (arg == null) throw new Exception("Expected a return register");
//                        if (((TargetRegister)arg).IsNewRegister) {
//                            registers.RemoveRegister((TargetRegister)arg);
//                        }
//                        jmp.Arg0 = arg.ToString();
//                        return instruct.Concat(jmp);
//                    case "<=": jmp.Op = OpCode.jmp_gt; break;
//                    case ">=": jmp.Op = OpCode.jmp_lt; break;
//                    case "<": jmp.Op = OpCode.jmp_gte; break;
//                    case ">": jmp.Op = OpCode.jmp_lte; break;
//                    case "==": jmp.Op = OpCode.jmp_neq; break;
//                    case "!=": jmp.Op = OpCode.jmp_eq; break;
//                    default:
//                        throw new Exception("Expected bool operator");
//                }

//                List<TargetRegister> borrowedRegisters = new();

//                // Arg1
//                TargetRegister? arg0 = null;
//                IEnumerable<ASM> instructions = CompileExpression(node.Params[0], ref arg0);
//                if (arg0 == null) throw new Exception("Expected a return register.");
//                if (((TargetRegister)arg0).IsNewRegister) {
//                    borrowedRegisters.Add((TargetRegister)arg0);
//                }
//                jmp.Arg0 = arg0.ToString();


//                // Arg2
//                TargetRegister? arg1 = null;
//                instructions = instructions.Concat(CompileExpression(node.Params[1], ref arg1));
//                if (arg1 == null) throw new Exception("Expected a return register.");
//                if (((TargetRegister)arg1).IsNewRegister) {
//                    borrowedRegisters.Add((TargetRegister)arg1);
//                }
//                jmp.Arg1 = arg1.ToString();

//                foreach (TargetRegister reg in borrowedRegisters) {
//                    registers.RemoveRegister(reg);
//                }
//                return instructions.Concat(jmp);
//            } else {
//                throw new Exception("Invalid condition");
//            }
//        }

//        private IEnumerable<ASM> CompileSelectionStatement(ASTNode node) {
//            if (node.Type != ASTType.SelectionStatement) throw new Exception("Expected selection type.");
//            if (node.Value != "if") {
//                throw new Exception("Unknown selection type.");
//            }

//            string else_label;
//            bool always_true;
//            bool always_false;
//            IEnumerable<ASM> instructions = GetJumpStatementFromCondition(node.Params[0], out else_label, out always_true, out always_false);

//            if (always_true) {
//                // Since the statement is always true, always run block A without adding any jumps
//                return instructions.Concat(CompileStatementBlock(node.Params[1]));
//            } else if (always_false) {
//                // Only applies for "else" or "else if" conditions.
//                // If statement always false, always run block B without adding any jumps
//                if (node.Params.Count > 2) {
//                    if (node.Params[2].Type == ASTType.SelectionStatement) {
//                        return instructions.Concat(CompileSelectionStatement(node.Params[2]));
//                    } else {
//                        return instructions.Concat(CompileStatementBlock(node.Params[2]));
//                    }
//                }
//                return instructions;
//            }

//            instructions = instructions.Concat(CompileStatementBlock(node.Params[1]));

//            if (node.Params.Count > 2) { // Else or else if
//                string label2 = registers.GetLabelName();

//                ASM jump2 = new();
//                jump2.Op = OpCode.jmp;
//                jump2.Arg0 = label2;
//                instructions = instructions.Concat(jump2);

//                ASM label1asm = new();
//                label1asm.Label = else_label;
//                instructions = instructions.Concat(label1asm);

//                // statement block
//                if (node.Params[2].Type == ASTType.SelectionStatement) {
//                    instructions = instructions.Concat(CompileSelectionStatement(node.Params[2]));
//                } else {
//                    instructions = instructions.Concat(CompileStatementBlock(node.Params[2]));
//                }
//                else_label = label2;
//            }

//            ASM exitLabel = new();
//            exitLabel.Label = else_label;
//            return instructions.Concat(exitLabel);
//        }

//    }
//}
