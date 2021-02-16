use crate::chunk::Chunk;
use crate::debug;

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

    #[test]
    fn create_vm() {
        let mut chunk = Chunk::new();
        let vm = VM::new();
    }
}