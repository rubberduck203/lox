use rs_lox::{Chunk, OpCode};
use rs_lox::debug;

fn main() {

    let mut chunk = Chunk::new();

    let constant = chunk.add_constant(1.2);
    chunk.write_opcode(OpCode::Constant);
    chunk.write_usize(constant);

    let constant = chunk.add_constant(22.0);
    chunk.write_opcode(OpCode::Constant);
    chunk.write_usize(constant);

    chunk.write_opcode(OpCode::Return);
    debug::disassemble_chunk(&chunk, "test chunk");
}
