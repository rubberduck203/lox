use crate::chunk::Chunk;
use crate::debug;

#[derive(Debug)]
pub enum InterpretError {
    Compile,
    Runtime(String),
}

pub struct VM{ //chunk must life at least as long as the VM
    ip: usize,
    // chunk: Option<&'a Chunk>
}

impl VM {
    pub fn new() -> VM {
        VM { ip: 0 }
    }

    pub fn interpret(&mut self, chunk: &Chunk) -> Result<(), InterpretError> {
        // self.chunk = Some(chunk);
        let instructions = chunk.bytes();
        loop {
            let instruction = instructions[self.ip];
            self.ip += 1;
            match instruction {
                0 => {
                    let constant = chunk.read_constant(self.ip);
                    self.ip += Chunk::constant_idx_size();
                    //TODO: something else...
                    debug::print_value(&constant);
                    println!();
                    return Ok(())
                }
                1 => return Ok(()),
                _ => return Err(InterpretError::Runtime(format!("Unknown opcode: {}", instruction)))
            }
        }
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