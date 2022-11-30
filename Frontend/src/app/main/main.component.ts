import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { CaffService } from '../services/caffservice';

@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.css']
})
export class MainComponent implements OnInit {
  searchFormControl = new FormControl('');
  caffs = [
    { id: "1", name: "1", uploaderid: "1", uploaderusername: "1" },
    { id: "1", name: "21", uploaderid: "1", uploaderusername: "1", },
    { id: "1", name: "31", uploaderid: "1", uploaderusername: "1", },
    { id: "1", name: "41", uploaderid: "1", uploaderusername: "1", },
    { id: "1", name: "51", uploaderid: "1", uploaderusername: "1", },
    { id: "1", name: "61", uploaderid: "1", uploaderusername: "1", },
    { id: "1", name: "71", uploaderid: "1", uploaderusername: "1", },
  ]

  constructor(private caffService: CaffService) { }


  filterCAFFs() {
    this.getCaffsByName(this.searchFormControl.value);
  }
  ngOnInit(): void {
    this.getCaffs();
  }
  getCaffs() {
    this.caffService.getCaffs(null).subscribe(res => {
      res.forEach(x => console.log(x.name));
      this.caffs = res;
    })
  }
  getCaffsByName(searchterm: string | null) {
    if (searchterm != null) {
      this.caffService.getCaffs(searchterm).subscribe(res => {
        this.caffs = res;
      })
    }

  }
  
  

}
