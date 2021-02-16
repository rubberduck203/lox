use crate::value::Value;
use std::convert::TryInto;

#[repr(u8)]
pub enum OpCode {
    Constant = 0,
    Return = 1
}

pub struct Chunk {
    code: Vec<u8> ,
    pub constants: Vec<Value>, //TODO: encapsulate
    pub lines: Vec<usize>
}

impl Chunk {
    pub fn new() -> Chunk {
        Chunk { code: vec![], constants: vec![], lines: vec![] }
    }

    pub fn write(&mut self, byte: u8, line: usize) {
        self.code.push(byte);
        self.lines.push(line);
    }

    pub fn write_opcode(&mut self, opcode: OpCode, line: usize)
    {
        self.write(opcode as u8, line);
    }

    pub fn write_usize(&mut self, value: usize, line: usize) {
        for byte in value.to_ne_bytes().iter() {
            self.write(*byte, line);
        }
    }

    //TODO: return opcodes and move iteration code from debug to here
    pub fn iter(&self) -> std::slice::Iter<u8> {
        self.code.iter()
    }

    pub fn bytes(&self) -> &Vec<u8> {
        &self.code
    }

    pub fn write_constant(&mut self, value: Value, line: usize) {
        let const_offset = self.add_constant(value);

        self.write_opcode(OpCode::Constant, line);
        self.write_usize(const_offset, line);
    }

    fn add_constant(&mut self, value: Value) -> usize {
        self.constants.push(value);
        self.constants.len() - 1
    }

    pub fn read_constant(&self, offset: usize) -> Value {
        let const_idx = self.constant_idx(offset);
        self.constants[const_idx]
    }

    pub fn constant_idx(&self, offset: usize) -> usize {
        // this function is exposed so we can use it in the debug module

        // instruction format := OpCode::Constant usize 
        // Read the next sizeOf(usize) bytes (pending on architecture)
        // The resulting usize is an index into the constants vector
        let end = offset + Chunk::constant_idx_size();
        let cbytes = &self.code[offset..end];
        usize::from_ne_bytes(cbytes.try_into().expect("Couldn't convert byte vector to usize."))
    }

    pub const fn constant_idx_size() -> usize {
        std::mem::size_of::<usize>()
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn write_byte() {
        let mut chunk = Chunk::new();
        chunk.write(OpCode::Return as u8, 1);
        assert_eq!(1, chunk.code[0]);
        assert_eq!(1, chunk.lines[0]);
    }

    #[test]
    fn write_opcode() {
        let mut chunk = Chunk::new();
        chunk.write_opcode(OpCode::Return, 1);
        assert_eq!(1, chunk.code[0]);
    }

    #[test]
    fn add_constant() {
        let mut chunk = Chunk::new();
        let idx = chunk.add_constant(23.0);
        assert_eq!(0, idx);
    }

    #[test]
    fn write_usize() {
        let mut chunk = Chunk::new();
        let constant = chunk.add_constant(1.2);
        chunk.write_opcode(OpCode::Constant, 1);
        assert_eq!(1, chunk.code.len());

        chunk.write_usize(constant, 2);

        assert_eq!(9, chunk.code.len());
        assert_eq!(1, chunk.lines[0]);
        assert_eq!(2, chunk.lines[1]);
        assert_eq!(2, chunk.lines[8]);
    }

    #[test]
    fn read_constant() {
        let mut chunk = Chunk::new();
        let const_offset = chunk.add_constant(1.2);
        chunk.write_opcode(OpCode::Constant, 1);
        chunk.write_usize(const_offset, 2);

        let actual = chunk.read_constant(0);
        assert_eq!(1.2, actual);
    }
}
