use crate::chunk::Chunk;
use crate::value::Value;
use crate::debug;

#[derive(Debug)]
pub enum InterpretError {
    Compile,
    Runtime(String),
}

pub struct VM{ //chunk must life at least as long as the VM
    ip: usize,
    stack: Vec<Value>
    // chunk: Option<&'a Chunk>
}

impl VM {
    pub fn new() -> VM{
        VM { ip: 0, stack: vec![] }
    }

    // fn reset_stack(&mut self) {
    //     self.stack.clear()
    // }

    pub fn interpret(&mut self, chunk: &Chunk) -> Result<(), InterpretError> {
        let instructions = chunk.bytes();
        loop {
            #[cfg(feature = "trace")]
            self.print_stack();
            #[cfg(feature = "trace")]
            debug::disassemble_instruction(chunk, self.ip);

            let instruction = instructions[self.ip];
            self.ip += 1;
            match instruction {
                0 => {
                    let constant = chunk.read_constant(self.ip);
                    self.ip += Chunk::constant_idx_size();
                    self.stack.push(constant);
                }
                1 => {
                    debug::print_value(&self.stack.pop().unwrap());
                    println!();
                    return Ok(())
                },
                _ => return Err(InterpretError::Runtime(format!("Unknown opcode: {}", instruction)))
            }
        }
    }

    #[cfg(feature = "trace")]
    fn print_stack(&self) {
        print!("\t");
        for slot in self.stack.iter() {
            print!("[ ");
            debug::print_value(slot);
            print!(" ]");
        }
        println!()
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::chunk::OpCode;

    #[test]
    fn create_vm() {
        let mut chunk = Chunk::new();
        let mut vm = VM::new();

        chunk.write_constant(1.2, 1);
        chunk.write_constant(1.2, 2);
        chunk.write_opcode(OpCode::Return, 2);

        assert_eq!((), vm.interpret(&chunk).unwrap())
    }
}