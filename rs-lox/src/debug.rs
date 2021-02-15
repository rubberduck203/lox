use crate::chunk::Chunk;
use crate::value::Value;
use std::convert::TryInto;

pub fn disassemble_chunk(chunk: &Chunk, name: &str) {
    println!("== {} ==", name);
    let bytes = chunk.bytes();

    let mut offset = 0;
    while offset < bytes.len() {
        offset = disassemble_instruction(chunk, offset);
    }
}

fn disassemble_instruction(chunk: &Chunk, offset: usize) -> usize {
    print!("{:04}\t", offset);
    let bytes = chunk.bytes();
    let instruction = bytes[offset];

    match instruction {
        0 => constant_instruction("Constant", chunk, offset),
        1 => simple_instruction("Return", offset),
        _ => {
            println!("Unknown opcode {}", instruction);
            offset + 1
        }
    }
}

fn simple_instruction(name: &str, offset: usize) -> usize {
    println!("{}", name);
    offset + 1
}

fn constant_instruction(name: &str, chunk: &Chunk, offset: usize) -> usize {
    let start = offset + 1;
    let constant_size = std::mem::size_of::<usize>();
    let end = start + constant_size;
    let cbytes = &chunk.bytes()[start..end];
    let const_idx = usize::from_ne_bytes(cbytes.try_into().unwrap());

    print!("{:<16} {:04} ", name, const_idx);
    print_value(&chunk.constants[const_idx]);
    println!();

    offset + 1 + constant_size
}

fn print_value(value: &Value) {
    print!("{}", value);
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::chunk::OpCode;

    #[test]
    fn disassemble_runs() {
        let mut chunk = Chunk::new();
        chunk.write_opcode(OpCode::Return);
        disassemble_chunk(&chunk, "a chunk");
    }
}