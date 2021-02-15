use crate::value::Value;

#[repr(u8)]
pub enum OpCode {
    Return = 0
}

pub struct Chunk {
    code: Vec<u8> ,
    constants: Vec<Value>
}

impl Chunk {
    pub fn new() -> Chunk {
        Chunk { code: vec![], constants: vec![] }
    }

    pub fn write(&mut self, byte: u8) {
        self.code.push(byte);
    }

    pub fn write_opcode(&mut self, opcode: OpCode)
    {
        self.write(opcode as u8);
    }

    //TODO: return opcodes and move iteration code from debug to here
    pub fn iter(&self) -> std::slice::Iter<u8> {
        self.code.iter()
    }

    pub fn bytes(&self) -> &Vec<u8> {
        &self.code
    }

    pub fn add_constant(&mut self, value: Value) -> usize {
        self.constants.push(value);
        self.constants.len() - 1
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn write_byte() {
        let mut chunk = Chunk::new();
        chunk.write(OpCode::Return as u8);
        assert_eq!(0, chunk.code[0]);
    }

    #[test]
    fn write_opcode() {
        let mut chunk = Chunk::new();
        chunk.write_opcode(OpCode::Return);
        assert_eq!(0, chunk.code[0]);
    }

    #[test]
    fn add_constant() {
        let mut chunk = Chunk::new();
        let idx = chunk.add_constant(23.0);
        assert_eq!(0, idx);
    }
}
