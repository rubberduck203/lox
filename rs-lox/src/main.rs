use rs_lox::{Chunk, OpCode, VM};
use rs_lox::debug;

fn main() {

    let mut chunk = Chunk::new();
    let mut vm = VM::new();

    chunk.write_constant(1.2, 1);
    chunk.write_constant(1.2, 2);
    chunk.write_opcode(OpCode::Return, 2);

    debug::disassemble_chunk(&chunk, "test chunk");
    vm.interpret(&chunk);
}
