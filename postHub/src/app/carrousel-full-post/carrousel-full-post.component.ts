import { Component, ElementRef, Input, QueryList, ViewChild, ViewChildren } from '@angular/core';
import Glide from '@glidejs/glide';
import { Picture } from '../models/picture';
import { PostService } from '../services/post.service';
import { FullPostComponent } from '../fullPost/fullPost.component';

@Component({
  selector: 'app-carrousel-full-post',
  templateUrl: './carrousel-full-post.component.html',
  styleUrls: ['./carrousel-full-post.component.css']
})
export class CarrouselFullPostComponent {

  @Input() listimages : Picture[] = [];

  @Input() toggleEdit : boolean = false;

  @ViewChild("commentWithPicture", {static:false}) pictureInput?: ElementRef;

  //@ViewChild("filesUploadByUser", {static:false}) pictureInput?: ElementRef;
  @ViewChildren('glideitems') glideitems : QueryList<any> = new QueryList();
  
  constructor(public postService : PostService, public postComponent : FullPostComponent){}

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

  public removepicture(id : number) {
    this.listimages.splice(id,1);
    this.initGlide();
  }

   //Delete a picture
   async DeletePicutre(id : number){
    if(this.postComponent.isAuthor != false){
      console.log("Est auteur")
      if(this.postComponent.post != undefined){
        if(id != undefined){
          for(let i = 0; i <= this.postComponent.listImages.length; i++){
            let image : Picture = this.listimages[i]
            if(image.id == id){
              this.listimages.splice(i, 1);
              break;
            }
          }
          this.postService.supprimerPhotoPost(this.postComponent.post?.id, id);
          console.log("Réussie")
        }
      }
    }
    console.log("N'est pas auteur")
  }
}
