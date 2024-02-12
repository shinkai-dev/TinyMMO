use std::net::SocketAddr;
use serde::Deserialize;
use strum::IntoEnumIterator;
use strum_macros::{Display};
use tokio_tungstenite::tungstenite::Message;
use crate::log::{Log, LogLevel};
use crate::objects::{Direction, get_player_obj_ids, ObjectId, slide_obj};

#[derive(Deserialize, PartialEq, Display)]
pub enum InputType {
    Move,
}

#[derive(Deserialize)]
pub struct PeerInput {
    input_type: InputType,
    args: String,
    actor_id: ObjectId
}

pub async fn process_peer_input(peer: SocketAddr, msg: Message) {
    let message = serde_json::from_str::<PeerInput>(&*msg.to_string()).unwrap();
    Log::new(LogLevel::Info, peer.ip().to_string() + " - " + &*message.input_type.to_string() +  " - " + &*message.args);
    let player_objs: Vec<String> = get_player_obj_ids(peer).await;
    if !player_objs.contains(&message.actor_id.to_string()) {
        Log::new(
            LogLevel::Suspect,
            peer.ip().to_string() + " - tried to act with " + &*message.actor_id +
                ", but he does not owns it"
        );
        return;
    }
    //it's very important to call the actions below with new threads, as the peer mutex will probably
    //cause a deadlock when sending the events back
    if message.input_type == InputType::Move {
        let direction = Direction::iter().nth(message.args.parse::<usize>().unwrap()).unwrap();
        tokio::spawn(slide_obj(message.actor_id, direction));
    }
}

