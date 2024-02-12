use std::collections::HashMap;
use std::fmt::Error;
use std::net::SocketAddr;
use std::sync::{Arc};
use std::thread::sleep;
use std::time::{Duration};
use serde::{Deserialize, Serialize, Serializer};
use tokio::net::TcpStream;
use tokio::sync::Mutex;
use tokio_tungstenite::WebSocketStream;
use crate::{create_obj_id};
use crate::event::{add_to_game, remove_from_game, send_update_obj_event};
use crate::state::{GAME_STATE, PEER_MAP, TICK_RATE_SECS};
use strum_macros::EnumIter;

pub type ObjectId = String;


#[derive(EnumIter, Deserialize, Clone)]
pub enum Movement {
    Idle,
    WalkNorth,
    WalkWest,
    WalkSouth,
    WalkEast,
    WalkNorthwest,
    WalkNortheast,
    WalkSouthwest,
    WalkSoutheast,
}

impl Serialize for Movement {
    fn serialize<S>(&self, serializer: S) -> Result<S::Ok, S::Error> where S: Serializer {
        let num = match &self {
            Movement::Idle => 0,
            Movement::WalkNorth => 1,
            Movement::WalkWest => 2,
            Movement::WalkSouth => 3,
            Movement::WalkEast => 4,
            Movement::WalkNorthwest => 5,
            Movement::WalkNortheast => 6,
            Movement::WalkSouthwest => 7,
            Movement::WalkSoutheast => 8,
        };
        serializer.serialize_i32(num)
    }
}

impl Movement {
    fn to_direction(self) -> Result<Direction, Error> {
        match self {
            Movement::WalkNorth => Ok(Direction::North),
            Movement::WalkWest => Ok(Direction::West),
            Movement::WalkSouth => Ok(Direction::South),
            Movement::WalkEast => Ok(Direction::East),
            Movement::WalkNorthwest => Ok(Direction::Northwest),
            Movement::WalkNortheast => Ok(Direction::Northeast),
            Movement::WalkSouthwest => Ok(Direction::Southwest),
            Movement::WalkSoutheast => Ok(Direction::Southeast),
            _ => Err(Error)
        }
    }
}

#[derive(Serialize, Deserialize, Clone)]
pub struct Progress {
    movement: Movement,
    ticks: u16,
    locked: bool,
    done: bool
}

impl Progress {
    fn new(movement: Movement, ticks: u16, locked: bool) -> Self {
        Progress {movement, locked, ticks, done: false}
    }

    fn new_idle() -> Self {
        Progress {movement: Movement::Idle, locked: false, ticks: 0, done: true}
    }

    fn wait_ticks(&self) {
        sleep(Duration::new((TICK_RATE_SECS as u16 * self.ticks) as u64, 0));
    }
}

#[derive(Serialize, Deserialize, Clone)]
pub enum ObjectModel {
    Player
}

#[derive(Serialize, Deserialize, Clone)]
pub struct Object {
    id: ObjectId,
    model: ObjectModel,
    owner: SocketAddr,
    transform: Transform,
    action: Progress
}

pub async fn get_player_obj_ids(peer: SocketAddr) -> Vec<String> {
    GAME_STATE.lock().await.iter()
        .filter(|(_, value)| value.get_owner().to_string() == peer.to_string())
        .map(|(id, _)| id.to_string())
        .collect::<Vec<String>>().try_into().unwrap()
}

pub async fn remove_player(peer: SocketAddr) {
    let obj_ids = get_player_obj_ids(peer).await;
    PEER_MAP.lock().await.remove(&peer);
    for id in obj_ids {
        remove_from_game(id).await.unwrap();
    }
}

async fn new_obj_with_id(id: ObjectId, owner: SocketAddr, model: ObjectModel) -> ObjectId {
    let obj = Object {
        transform: Transform::new(0, 0, 0),
        owner, model, id: id.clone(),
        action: Progress::new_idle()
    };
    add_to_game(obj).await.unwrap();
    id
}

impl Object {
    pub async fn new(&self, owner: SocketAddr, model: ObjectModel) -> ObjectId {
        new_obj_with_id(create_obj_id(), owner, model).await
    }

    pub async fn new_with_id(id: ObjectId, owner: SocketAddr, model: ObjectModel) -> ObjectId {
        new_obj_with_id(id, owner, model).await
    }

    pub async fn update(&self) {
        send_update_obj_event(serde_json::to_string(self).unwrap()).await.unwrap();
    }

    pub async fn set_pos(&mut self, pos: Position) {
        self.transform.position = pos;
        self.update().await;
    }

    pub async fn set_x(&mut self, x: i16) {
        self.transform.position.x = x;
        self.update().await;
    }

    pub async fn set_y(&mut self, y: i16) {
        self.transform.position.y = y;
        self.update().await;
    }

    pub async fn set_z(&mut self, z: i16) {
        self.transform.position.z = z;
        self.update().await;
    }

    pub async fn sum_x(&mut self, x: i16) {
        self.transform.position.x += x;
        self.update().await;
    }

    pub async fn sum_y(&mut self, y: i16) {
        self.transform.position.y += y;
        self.update().await;
    }

    pub async fn sum_z(&mut self, z: i16) {
        self.transform.position.z += z;
        self.update().await;
    }

    pub async fn act(&mut self, movement: Movement, ticks: u16, locked: bool) {
        self.action = Progress::new(movement, ticks, locked);
        self.update().await;
    }

    pub async fn wait(&mut self) {
        if self.action.done {return;}

        self.action.wait_ticks();
        self.action.movement = Movement::Idle;
        self.update().await;
    }

    pub async fn act_and_wait(&mut self, movement: Movement, ticks: u16, locked: bool) {
        self.act(movement, ticks, locked).await;
        self.wait().await;
    }

    pub fn get_owner(&self) -> &SocketAddr {
        &self.owner
    }

    pub fn get_id(&self) -> ObjectId {
        self.id.clone()
    }
}


#[derive(Serialize, Deserialize, Clone)]
pub struct Position {
    x: i16,
    y: i16,
    z: i16
}

impl Position {
    pub fn new(x: i16, y: i16, z: i16) -> Self {
        Self {x, y, z}
    }
}

#[derive(Serialize, Deserialize, Clone)]
pub struct Transform {
    position: Position
}

#[derive(EnumIter, PartialEq, Clone)]
pub enum Direction {
    North,
    West,
    South,
    East,
    Northwest,
    Northeast,
    Southwest,
    Southeast
}

impl Direction {
    fn to_movement(&self) -> Movement {
        match self {
            Direction::North => Movement::WalkNorth,
            Direction::West => Movement::WalkWest,
            Direction::South => Movement::WalkSouth,
            Direction::East => Movement::WalkEast,
            Direction::Northwest => Movement::WalkNorthwest,
            Direction::Northeast => Movement::WalkNortheast,
            Direction::Southwest => Movement::WalkSouthwest,
            Direction::Southeast => Movement::WalkSoutheast,
        }
    }
}

impl Transform {
    pub fn new(x: i16, y: i16, z: i16) -> Self {
        Self {position: Position::new(x, y, z)}
    }
}

async fn raw_move_obj(obj: &mut Object, direction: Direction) {
    if direction == Direction::North {
        obj.sum_z(-1).await;
    }
    if direction == Direction::West  {
        obj.sum_x(-1).await;
    }
    if direction == Direction::South  {
        obj.sum_z(1).await;
    }
    if direction == Direction::East  {
        obj.sum_x(1).await;
    }
}

pub async fn move_obj(obj_id: ObjectId, direction: Direction) {
    let mut game_state = GAME_STATE.lock().await;
    let obj = game_state.get_mut(&*obj_id).unwrap();
    raw_move_obj(obj, direction).await;
}

pub async fn slide_obj(obj_id: ObjectId, direction: Direction) {
    let mut game_state = GAME_STATE.lock().await;
    let obj = game_state.get_mut(&*obj_id).unwrap();
    obj.act_and_wait(direction.to_movement(), 1, false).await;
    raw_move_obj(obj, direction).await;
}

pub type GameState = Arc<Mutex<HashMap<ObjectId, Object>>>;
pub type PeerMap = Arc<Mutex<HashMap<SocketAddr, Arc<Mutex<WebSocketStream<TcpStream>>>>>>;
