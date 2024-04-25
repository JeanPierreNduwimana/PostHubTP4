import { Component, ElementRef, Input, QueryList, ViewChild, ViewChildren } from '@angular/core';
import Glide from '@glidejs/glide';
import { Picture } from '../models/picture';

@Component({
  selector: 'app-carrousel-full-post',
  templateUrl: './carrousel-full-post.component.html',
  styleUrls: ['./carrousel-full-post.component.css']
})
export class CarrouselFullPostComponent {

  @Input() listimages : Picture[] = [];

  @ViewChild("commentWithPicture", {static:false}) pictureInput?: ElementRef;

  //@ViewChild("filesUploadByUser", {static:false}) pictureInput?: ElementRef;
  @ViewChildren('glideitems') glideitems : QueryList<any> = new QueryList();
  
  ngAfterViewInit() {
    this.glideitems.changes.subscribe(e => {
      this.initGlide();
    });
    
    if (this.glideitems.length > 0) {
      this.initGlide();
    }
  }
    

  initGlide() {
    console.log(this.listimages);
    var glide = new Glide('.glide', {
      type: 'carousel',
      focusAt: 'center',
      perView: Math.ceil(window.innerWidth / 400)
    });
    glide.mount();
  }
}
