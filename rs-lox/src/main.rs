use rs_lox::{Chunk, OpCode};
use rs_lox::debug;

fn main() {

    let mut chunk = Chunk::new();

    let constant = chunk.add_constant(1.2);
    chunk.write_opcode(OpCode::Constant, 1);
    chunk.write_usize(constant, 1);

    let constant = chunk.add_constant(22.0);
    chunk.write_opcode(OpCode::Constant, 2);
    chunk.write_usize(constant, 2);

    chunk.write_opcode(OpCode::Return, 2);
    debug::disassemble_chunk(&chunk, "test chunk");
}
