use crate::chunk::Chunk;
use crate::value::Value;

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

    if offset > 0 && chunk.lines[offset] == chunk.lines[offset - 1] {
        print!("   | ");
    } else {
        print!("{:04} ", chunk.lines[offset]);
    }

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
    // skip the opcode
    let data_offset = offset + 1;

    print!("{:<16} {:04} ", name, chunk.constant_idx(data_offset));
    print_value(&chunk.read_constant(data_offset));
    println!();

    let constant_size = Chunk::constant_idx_size();
    offset + 1 + constant_size
}

pub fn print_value(value: &Value) {
    print!("{}", value);
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::chunk::OpCode;

    #[test]
    fn disassemble_runs() {
        let mut chunk = Chunk::new();
        chunk.write_opcode(OpCode::Return, 1);
        disassemble_chunk(&chunk, "a chunk");
    }
}