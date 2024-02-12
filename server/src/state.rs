use std::collections::HashMap;
use std::sync::atomic::{AtomicUsize, Ordering};
use once_cell::sync::Lazy;
use tokio::sync::Mutex;
use crate::objects::{GameState, PeerMap};

pub static GAME_STATE: Lazy<GameState> = Lazy::new(|| {
    GameState::new(Mutex::new(HashMap::new()))
});
pub static PEER_MAP: Lazy<PeerMap> = Lazy::new(|| {
    PeerMap::new(Mutex::new(HashMap::new()))
});
pub static ID_COUNTER: AtomicUsize = AtomicUsize::new(0);

pub fn create_obj_id() -> String {
    ID_COUNTER.fetch_add(1, Ordering::Relaxed).to_string()
}

//those shall be the same in the client code:
pub static TICK_RATE_SECS: f32 = 0.2;
