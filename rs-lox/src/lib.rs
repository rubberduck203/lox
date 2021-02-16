mod chunk;
pub use chunk::*;

mod value;
pub use value::*;

mod vm;
pub use vm::*;

pub mod debug;

#[cfg(test)]
mod tests {
    // #[test]
    // fn it_works() {
    //     assert_eq!(2 + 2, 4);
    // }
}
