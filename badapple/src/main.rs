use std::{fs, io::Cursor};

use rayon::iter::{IntoParallelIterator, ParallelIterator};
use serde::Serialize;

fn main() {
  println!("Hello, world!");

  let mut images = (1..=6572)
    .into_par_iter()
    .map(|i| {
      let name = format!("bad-apple-frames/frames/{i:03}.jpg");
      // println!("reading {name:?}");
      let img = fs::read(name).unwrap();
      single_image(&img)
    })
    .collect::<Vec<_>>();

  fs::write("badapple.json", serde_json::to_vec(&images).unwrap()).unwrap();
}

fn single_image(image: &[u8]) -> Vec<ContourJson> {
  let img = image::load(Cursor::new(image), image::ImageFormat::Jpeg).unwrap();
  let mut img = img.to_luma8();

img.as_flat_samples_mut().as_mut_slice().iter_mut().for_each(|x| {
  *x = if *x > 127 { 255 } else { 0 }
});

  let contours = imageproc::contours::find_contours_with_threshold::<u32>(&img, 127);
  // println!("{:#?}", contours);
  let contours = contours
    .into_iter()
    .map(|c| ContourJson {
      points: c
        .points
        .into_iter()
        .map(|p| Vec2 {
          x: p.x as f32,
          y: p.y as f32,
        })
        .collect(),
      is_hole: match c.border_type {
        imageproc::contours::BorderType::Outer => false,
        imageproc::contours::BorderType::Hole => true,
      },
    })
    .collect::<Vec<_>>();

  contours
}

#[derive(Serialize)]
pub struct ContourJson {
  points: Vec<Vec2>,
  is_hole: bool,
}
#[derive(Serialize)]

pub struct Vec2 {
  #[serde(rename = "X")]
  x: f32,
  #[serde(rename = "Y")]
  y: f32,
}
