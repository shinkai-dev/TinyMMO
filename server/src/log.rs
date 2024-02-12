use std::fmt::{Display, Formatter};
use tokio_tungstenite::tungstenite::Error;

pub enum LogLevel {
    Info,
    Debug,
    Error,
    Suspect
}

impl Display for LogLevel {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        match self {
            LogLevel::Error => write!(f, "error"),
            LogLevel::Debug => write!(f, "debug"),
            LogLevel::Info => write!(f, "info"),
            LogLevel::Suspect => write!(f, "suspect")
        }
    }
}

pub struct Log {
    level: LogLevel,
    message: String,
}

impl Log {
    pub fn new(level: LogLevel, message: String) -> Self {
        display(&level, &message);
        Self{level, message: message.to_string()}
    }

    pub fn new_error(err: Error) -> Self {
        let message = err.to_string();
        display(&LogLevel::Error, &message);
        Self{level: LogLevel::Error, message}
    }
}

fn display(level: &LogLevel, message: &String) {
    eprintln!("[{}]\t{}", level, message);
}