import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Caff } from '../models/caff';
import { CaffService } from '../services/caffservice';

@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.css']
})
export class MainComponent implements OnInit {
  searchFormControl = new FormControl('');
  caffs: Caff[] = [];

  constructor(private caffService: CaffService) { }

  filterCAFFs() {
    this.getCaffsByName(this.searchFormControl.value);
  }
  ngOnInit(): void {
    this.getCaffs();
  }
  getCaffs() {
    this.caffService.getCaffs(null).subscribe(res => {
      this.caffs = res;
      //startTimer();

    })
  }
  getCaffsByName(searchterm: string | null) {
    if (searchterm != null) {
      this.caffService.getCaffs(searchterm).subscribe(res => {
        this.caffs = res;
        //startTimer();

      })
    }
  }

  getPreview(id: string) {
    return this.caffService.getCaffPreview(id);
  }

  getPreviewGif(id:string) {
    let url = "https://localhost:7235/api/caffs/" + id + "/preview";;
    return url;
  }

  clearFormControl() {
    this.searchFormControl.setValue("");
    this.getCaffs();
  }

}


/*function startTimer() {
    console.log("started");
  const gif = document.getElementById("gif");
    gif!.setAttribute("src", gif!.getAttribute('src')!.toString());
    setTimeout(function () { startTimer() }, 5000);
}




function restartGIF(gif: HTMLElement) {
  gif!.setAttribute("src", gif.getAttribute('src')!.toString());
  console.log("gifs restarted");
}
*/
