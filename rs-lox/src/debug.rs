use crate::chunk::Chunk;

pub fn disassemble_chunk(chunk: &Chunk, name: &str) {
    println!("== {} ==", name);
    let bytes = chunk.bytes();

    let mut offset = 0;
    while offset < bytes.len() {
        offset = disassemble_instruction(bytes, offset);
    }
}

fn disassemble_instruction(bytes: &Vec<u8>, offset: usize) -> usize {
    print!("{:04}\t", offset);
    let instruction = bytes[offset];

    match instruction {
        0 => simple_instruction("Return", offset),
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