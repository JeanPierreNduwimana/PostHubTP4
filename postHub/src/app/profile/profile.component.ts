import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { UserService } from '../services/user.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {

  userIsConnected : boolean = false;

  // Vous êtes obligés d'utiliser ces trois propriétés
  oldPassword : string = "";
  newPassword : string = "";
  newPasswordConfirm : string = "";

  username : string | null = null;

  constructor(public userService : UserService) { }

  ngOnInit() {
    this.userIsConnected = localStorage.getItem("token") != null;
    this.username = localStorage.getItem("username");
  }

  @ViewChild("fileUploadViewChild", {static:false}) pictureInput?: ElementRef;

  async ChangeAvatar(){
    let formdata = new FormData;
    let file = this.pictureInput?.nativeElement.filesUploadByUser;
    console.log(file);
    if(file != null){
      formdata.append("UserNewAvatar", file, file.name)
      if(this.username != null){
        await this.userService.changeAvatar(this.username, formdata)
      }
    }
  }

}
