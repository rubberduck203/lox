use rs_lox::{Chunk, OpCode};
use rs_lox::debug;

fn main() {

    let mut chunk = Chunk::new();
    chunk.write_opcode(OpCode::Return);
    debug::disassemble_chunk(&chunk, "test chunk");
}
