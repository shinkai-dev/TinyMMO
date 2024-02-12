use std::net::SocketAddr;
use std::ops::Deref;
use futures_util::SinkExt;
use tokio::time::error::Error;
use crate::{GAME_STATE, PEER_MAP};
use crate::objects::Object;

pub enum Action {
    Spawn,
    Destroy,
    SendState,
    Update
}

impl ToString for Action {
    fn to_string(&self) -> String {
        match &self {
            Action::Spawn => "0",
            Action::Destroy => "1",
            Action::SendState => "2",
            Action::Update => "3",
        }.parse().unwrap()
    }
}

pub async fn remove_from_game(id: String) -> Result<(), Error> {
    let peer_map = PEER_MAP.lock().await;
    let mut game_state = GAME_STATE.lock().await;
    game_state.remove(id.as_str());
    for (_, stream) in peer_map.iter() {
        stream.lock().await.send((Action::Destroy.to_string() + id.as_str()).into()).await.unwrap();
    }

    Ok(())
}

pub async fn add_to_game(object: Object) -> Result<(), Error> {
    let mut game_state = GAME_STATE.lock().await;
    let obj_id = object.get_id();
    let obj_json = serde_json::to_string(&object).unwrap();
    game_state.insert(obj_id.clone(), object.clone());
    send_event_global(Action::Spawn, obj_json).await.unwrap();

    Ok(())
}

/// Make sure to send the object id
pub async fn send_update_obj_event(json: String) -> Result<(), Error> {
    send_event_global(Action::Update, json).await.unwrap();

    Ok(())
}

pub async fn send_event_global(action: Action, arg: String) -> Result<(), Error> {
    for (_, stream) in PEER_MAP.lock().await.iter() {
        stream.lock().await.send((action.to_string() + arg.as_str()).into()).await.unwrap();
    }

    Ok(())
}

pub async fn send_event(peer_socket: SocketAddr, action: Action, arg: String) -> Result<(), Error> {
    send_simple_message(peer_socket, action.to_string() + arg.as_str()).await.unwrap();

    Ok(())
}

pub async fn send_simple_message(peer_socket: SocketAddr, arg: String) -> Result<(), Error> {
    let peer_map = PEER_MAP.lock().await;
    let mut peer = peer_map.get(&peer_socket).unwrap().lock().await;
    peer.send(arg.as_str().into()).await.unwrap();

    Ok(())
}

pub async fn send_game_state(peer_socket: SocketAddr) -> Result<(), Error> {
    let game_state = GAME_STATE.lock().await;
    let game_state_json = serde_json::to_string(&game_state.deref()).unwrap();
    send_event(peer_socket, Action::SendState, game_state_json).await.unwrap();

    Ok(())
}